using System.Data;
using System.Text.RegularExpressions;
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
public sealed class ExternalInventoryController(IConfiguration configuration, CrmDbContext db) : ControllerBase
{
    private const int MaxTake = 200;
    private const string InventoryView = "[Inventariomotosycarros].[dbo].[INVENTARIO_EXISTENCIA]";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ExternalInventoryItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? warehouse,
        [FromQuery] bool availableOnly = true,
        [FromQuery] int take = 80,
        CancellationToken cancellationToken = default)
    {
        var rows = await ReadExternalInventoryAsync(search, warehouse, availableOnly, Math.Clamp(take, 1, MaxTake), cancellationToken);
        var productReferences = rows
            .Select(x => x.Code)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var products = await db.Productos
            .Where(x => productReferences.Contains(x.Referencia))
            .ToListAsync(cancellationToken);
        var productsByReference = products
            .GroupBy(x => x.Referencia, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        return Ok(rows.Select(row =>
        {
            productsByReference.TryGetValue(row.Code, out var product);
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
                product?.Precio,
                product is not null && product.Activo);
        }).ToList());
    }

    [HttpGet("warehouses")]
    public async Task<ActionResult<IReadOnlyCollection<object>>> Warehouses(CancellationToken cancellationToken)
    {
        var rows = await ReadExternalInventoryAsync(null, null, false, MaxTake, cancellationToken);
        return Ok(rows
            .GroupBy(x => new { x.WarehouseCode, x.WarehouseName })
            .Select(x => new { code = x.Key.WarehouseCode, name = x.Key.WarehouseName, quantity = x.Sum(row => row.Quantity) })
            .OrderBy(x => x.name)
            .ToList());
    }

    private async Task<IReadOnlyCollection<ExternalInventoryRow>> ReadExternalInventoryAsync(string? search, string? warehouse, bool availableOnly, int take, CancellationToken cancellationToken)
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
            FROM {InventoryView}
            WHERE (@AvailableOnly = 0 OR ISNULL(Existencias, 0) > 0)
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

    private static string ReadString(IDataRecord reader, string name) =>
        reader[name] == DBNull.Value ? string.Empty : Convert.ToString(reader[name])?.Trim() ?? string.Empty;

    private static string? ReadNullableString(IDataRecord reader, string name)
    {
        var value = ReadString(reader, name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

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

    private sealed record ExternalInventoryRow(string WarehouseCode, string WarehouseName, string Code, string Name, string? Presentation, string? SerialNumber, int Quantity);
}
