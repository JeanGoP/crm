using System.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/external-inventory")]
public sealed class ExternalInventoryController(IConfiguration configuration, CrmDbContext db, ITenantContext tenantContext) : ControllerBase
{
    private const int MaxTake = 200;
    private const string InventorySchema = "dbo";
    private const string InventoryView = "INVENTARIO_EXISTENCIA";
    private const string WarehouseTable = "Bodega";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ExternalInventoryItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? warehouse,
        [FromQuery] bool availableOnly = true,
        [FromQuery] int take = 80,
        CancellationToken cancellationToken = default)
    {
        var inventoryConfig = await GetCompanyInventoryConfigAsync(cancellationToken);
        if (inventoryConfig.AllowedWarehouses.Count == 0 || string.IsNullOrWhiteSpace(inventoryConfig.DatabaseName))
        {
            return Ok(Array.Empty<ExternalInventoryItemDto>());
        }

        var rows = await ReadExternalInventoryAsync(search, warehouse, availableOnly, Math.Clamp(take, 1, MaxTake), inventoryConfig, cancellationToken);
        var productReferences = rows
            .Select(x => x.Code)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var products = await db.Productos
            .Include(x => x.PreciosPorSede)
            .Where(x => productReferences.Contains(x.Referencia))
            .ToListAsync(cancellationToken);
        var productsByReference = products
            .GroupBy(x => x.Referencia, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        return Ok(rows.Select(row =>
        {
            productsByReference.TryGetValue(row.Code, out var product);
            var productPrice = product is null ? (decimal?)null : ResolveProductPrice(product, inventoryConfig.SalesPointId);
            var isQuoteReady = product is not null && product.Activo && productPrice.GetValueOrDefault() > 0;
            var (engine, chassis) = ParseSerial(row.SerialNumber);
            return new ExternalInventoryItemDto(
                row.WarehouseCode,
                row.WarehouseName,
                row.Code,
                row.Name,
                row.Presentation,
                row.SerialNumber,
                engine,
                chassis,
                row.Quantity,
                product?.Id,
                product is null ? null : ProductName(product),
                productPrice,
                product?.Activo ?? false,
                isQuoteReady);
        }).ToList());
    }

    [HttpGet("warehouses")]
    public async Task<ActionResult<IReadOnlyCollection<object>>> Warehouses(CancellationToken cancellationToken)
    {
        var inventoryConfig = await GetCompanyInventoryConfigAsync(cancellationToken);
        if (inventoryConfig.AllowedWarehouses.Count == 0 || string.IsNullOrWhiteSpace(inventoryConfig.DatabaseName))
        {
            return Ok(Array.Empty<object>());
        }

        var rows = await ReadExternalInventoryAsync(null, null, false, MaxTake, inventoryConfig, cancellationToken);
        return Ok(rows
            .GroupBy(x => new { x.WarehouseCode, x.WarehouseName })
            .Select(x => new { code = x.Key.WarehouseCode, name = x.Key.WarehouseName, quantity = x.Sum(row => row.Quantity) })
            .OrderBy(x => x.name)
            .ToList());
    }

    [HttpGet("warehouse-catalog")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<IReadOnlyCollection<ExternalInventoryWarehouseDto>>> WarehouseCatalog(
        [FromQuery] string? databaseName,
        CancellationToken cancellationToken)
    {
        var normalizedDatabase = NormalizeDatabaseName(databaseName);
        if (string.IsNullOrWhiteSpace(normalizedDatabase))
        {
            normalizedDatabase = await GetCompanyDatabaseNameAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(normalizedDatabase))
        {
            return BadRequest(new { detail = "Configure primero la base de datos de inventario en la empresa." });
        }

        return Ok(await ReadWarehouseCatalogAsync(normalizedDatabase, cancellationToken));
    }

    private async Task<IReadOnlyCollection<ExternalInventoryRow>> ReadExternalInventoryAsync(string? search, string? warehouse, bool availableOnly, int take, ExternalInventoryConfig inventoryConfig, CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("ExternalInventoryConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No hay cadena de conexion configurada para el inventario externo.");
        }

        var normalizedSearch = Normalize(search);
        var normalizedWarehouse = Normalize(warehouse);
        var warehouseParameters = inventoryConfig.AllowedWarehouses.Select((_, index) => $"@AllowedWarehouse{index}").ToArray();
        var inventoryView = BuildInventoryViewName(inventoryConfig.DatabaseName);
        var rows = new List<ExternalInventoryRow>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"""
            SELECT TOP (@Take)
                Bodega,
                Codigo,
                Nombre,
                Presentacion,
                NumerodeSerie,
                Existencias,
                NombreBodega
            FROM {inventoryView}
            WHERE (@AvailableOnly = 0 OR ISNULL(Existencias, 0) > 0)
              AND Bodega IN ({string.Join(", ", warehouseParameters)})
              AND (@Warehouse = '' OR Bodega = @Warehouse OR NombreBodega LIKE @WarehouseLike)
              AND (
                    @Search = ''
                    OR Codigo LIKE @Like
                    OR Nombre LIKE @Like
                    OR Presentacion LIKE @Like
                    OR NumerodeSerie LIKE @Like
                    OR NombreBodega LIKE @Like
                  )
            ORDER BY Nombre, Codigo, NombreBodega, NumerodeSerie;
            """;
        command.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        command.Parameters.Add(new SqlParameter("@AvailableOnly", SqlDbType.Bit) { Value = availableOnly });
        command.Parameters.Add(new SqlParameter("@Search", SqlDbType.VarChar, 120) { Value = normalizedSearch });
        command.Parameters.Add(new SqlParameter("@Like", SqlDbType.VarChar, 140) { Value = $"%{normalizedSearch}%" });
        command.Parameters.Add(new SqlParameter("@Warehouse", SqlDbType.VarChar, 40) { Value = normalizedWarehouse });
        command.Parameters.Add(new SqlParameter("@WarehouseLike", SqlDbType.VarChar, 140) { Value = $"%{normalizedWarehouse}%" });
        foreach (var (allowedWarehouse, index) in inventoryConfig.AllowedWarehouses.Select((value, index) => (value, index)))
        {
            command.Parameters.Add(new SqlParameter($"@AllowedWarehouse{index}", SqlDbType.VarChar, 40) { Value = allowedWarehouse });
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ExternalInventoryRow(
                ReadString(reader, "Bodega"),
                ReadString(reader, "NombreBodega"),
                ReadString(reader, "Codigo"),
                ReadString(reader, "Nombre"),
                ReadNullableString(reader, "Presentacion"),
                ReadNullableString(reader, "NumerodeSerie"),
                reader["Existencias"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Existencias"])));
        }

        return rows;
    }

    private async Task<IReadOnlyCollection<ExternalInventoryWarehouseDto>> ReadWarehouseCatalogAsync(string databaseName, CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("ExternalInventoryConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No hay cadena de conexion configurada para el inventario externo.");
        }

        var warehouses = new List<ExternalInventoryWarehouseDto>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"""
            SELECT TOP (500)
                Codigo,
                Nombre
            FROM {BuildWarehouseTableName(databaseName)}
            WHERE Codigo IS NOT NULL
            ORDER BY Nombre, Codigo;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var code = ReadString(reader, "Codigo");
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }

            warehouses.Add(new ExternalInventoryWarehouseDto(code, ReadString(reader, "Nombre")));
        }

        return warehouses;
    }

    private async Task<ExternalInventoryConfig> GetCompanyInventoryConfigAsync(CancellationToken cancellationToken)
    {
        if (tenantContext.EmpresaId is not Guid companyId)
        {
            return new ExternalInventoryConfig(null, [], null);
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return new ExternalInventoryConfig(null, [], null);
        }

        var databaseName = await GetCompanyDatabaseNameAsync(cancellationToken);
        var warehouseCodes = await db.Usuarios
            .Where(x => x.Id == userId && x.EmpresaId == companyId && x.PuntoVentaId.HasValue)
            .Select(x => new { x.PuntoVentaId, x.PuntoVenta!.BodegasInventarioExterno })
            .FirstOrDefaultAsync(cancellationToken);

        return warehouseCodes is null
            ? new ExternalInventoryConfig(databaseName, [], null)
            : new ExternalInventoryConfig(databaseName, ParseWarehouseCodes(warehouseCodes.BodegasInventarioExterno), warehouseCodes.PuntoVentaId);
    }

    private async Task<string?> GetCompanyDatabaseNameAsync(CancellationToken cancellationToken)
    {
        if (tenantContext.EmpresaId is not Guid companyId)
        {
            return null;
        }

        var databaseName = await db.Empresas
            .IgnoreQueryFilters()
            .Where(x => x.Id == companyId)
            .Select(x => x.BaseDatosInventarioExterno)
            .FirstOrDefaultAsync(cancellationToken);

        return NormalizeDatabaseName(databaseName);
    }

    private static string ReadString(IDataRecord reader, string name) =>
        reader[name] == DBNull.Value ? string.Empty : Convert.ToString(reader[name])?.Trim() ?? string.Empty;

    private static string? ReadNullableString(IDataRecord reader, string name)
    {
        var value = ReadString(reader, name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string BuildInventoryViewName(string? databaseName)
    {
        var normalizedDatabase = NormalizeDatabaseName(databaseName);
        if (string.IsNullOrWhiteSpace(normalizedDatabase))
        {
            throw new InvalidOperationException("No hay base de datos de inventario configurada para la empresa.");
        }

        return $"[{normalizedDatabase}].[{InventorySchema}].[{InventoryView}]";
    }

    private static string BuildWarehouseTableName(string? databaseName)
    {
        var normalizedDatabase = NormalizeDatabaseName(databaseName);
        if (string.IsNullOrWhiteSpace(normalizedDatabase))
        {
            throw new InvalidOperationException("No hay base de datos de inventario configurada para consultar bodegas.");
        }

        return $"[{normalizedDatabase}].[{InventorySchema}].[{WarehouseTable}]";
    }

    private static string? NormalizeDatabaseName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var name = value.Trim();
        if (name.Length > 128 || !name.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            throw new InvalidOperationException("La base de datos de inventario configurada para la empresa no es valida.");
        }

        return name;
    }

    private static IReadOnlyCollection<string> ParseWarehouseCodes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value
            .Split([',', ';', '|', '\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static (string? Engine, string? Chassis) ParseSerial(string? serial)
    {
        if (string.IsNullOrWhiteSpace(serial)) return (null, null);
        var engine = Regex.Match(serial, @"\bMOT\s+([A-Z0-9]+)", RegexOptions.IgnoreCase).Groups[1].Value;
        var chassis = Regex.Match(serial, @"\bCHA\s+([A-Z0-9]+)", RegexOptions.IgnoreCase).Groups[1].Value;
        return (string.IsNullOrWhiteSpace(engine) ? null : engine, string.IsNullOrWhiteSpace(chassis) ? null : chassis);
    }

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }

    private static decimal ResolveProductPrice(Producto product, Guid? salesPointId)
    {
        if (salesPointId.HasValue && IsApplianceCategory(product.Categoria))
        {
            var salesPointPrice = product.PreciosPorSede
                .Where(x => x.Activo && x.PuntoVentaId == salesPointId.Value)
                .OrderByDescending(x => x.VigenteDesde ?? DateTime.MinValue)
                .FirstOrDefault();
            if (salesPointPrice is not null) return salesPointPrice.Precio;
        }

        return product.Precio;
    }

    private static bool IsApplianceCategory(string category) =>
        category.Contains("electrodom", StringComparison.OrdinalIgnoreCase);

    private sealed record ExternalInventoryRow(string WarehouseCode, string WarehouseName, string Code, string Name, string? Presentation, string? SerialNumber, int Quantity);
    private sealed record ExternalInventoryConfig(string? DatabaseName, IReadOnlyCollection<string> AllowedWarehouses, Guid? SalesPointId);
}
