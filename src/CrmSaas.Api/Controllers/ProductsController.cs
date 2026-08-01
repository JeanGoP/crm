using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(CrmDbContext db, IConfiguration configuration, ITenantContext tenantContext) : ControllerBase
{
    private const string ExternalInventoryCategory = "Inventario externo";
    private const int MaxExternalInventorySyncRows = 1500;
    private const string InventorySchema = "dbo";
    private const string InventoryView = "INVENTARIO_EXISTENCIA";
    private static readonly string[] ImportHeaders =
    [
        "Nombre",
        "Categoria",
        "Marca",
        "Modelo",
        "Linea",
        "Version",
        "Referencia",
        "Descripcion",
        "Cilindraje",
        "Ano",
        "Color",
        "Precio",
        "SOAT",
        "Matricula",
        "Impuestos",
        "FichaTecnica",
        "VigenteDesde",
        "Activo"
    ];

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> Get(CancellationToken cancellationToken)
    {
        var inventoryConfig = await GetCurrentUserInventoryConfigAsync(cancellationToken);
        var query = db.Productos
            .Include(x => x.Fotos)
            .Include(x => x.PreciosPorSede).ThenInclude(x => x.PuntoVenta)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(inventoryConfig.DatabaseName))
        {
            if (inventoryConfig.AllowedWarehouses.Count == 0)
            {
                return Ok(Array.Empty<ProductDto>());
            }

            var externalReferences = (await ReadExternalInventoryProductsAsync(inventoryConfig, cancellationToken))
                .Select(x => x.Code)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (externalReferences.Length == 0)
            {
                return Ok(Array.Empty<ProductDto>());
            }

            query = query.Where(x => externalReferences.Contains(x.Referencia));
        }

        var products = await query
            .OrderBy(x => x.Categoria).ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
        return Ok(products.Select(ToDto).ToList());
    }

    [HttpGet("import-template")]
    [Authorize(Roles = "Administrador")]
    public ActionResult DownloadImportTemplate()
    {
        var csv = "\uFEFFsep=;" + Environment.NewLine + string.Join(';', ImportHeaders) + Environment.NewLine;
        return File(Encoding.UTF8.GetBytes(csv), "text/csv; charset=utf-8", "plantilla_productos.csv");
    }

    [HttpPost("import")]
    [Authorize(Roles = "Administrador")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<object>> Import([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) throw new ValidationException("Debe seleccionar un archivo CSV.");
        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Por ahora la carga masiva recibe archivos CSV compatibles con Excel.");
        }

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine)) throw new ValidationException("El archivo no tiene encabezados.");

        var separator = DetectSeparator(headerLine);
        if (headerLine.Trim().StartsWith("sep=", StringComparison.OrdinalIgnoreCase))
        {
            var configuredSeparator = headerLine.Trim()[4..].Trim();
            separator = string.IsNullOrEmpty(configuredSeparator) ? separator : configuredSeparator[0];
            headerLine = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(headerLine)) throw new ValidationException("El archivo no tiene encabezados.");
        }

        var headers = ParseCsvLine(headerLine, separator).Select(NormalizeHeader).ToArray();
        var headerMap = headers
            .Select((header, index) => new { header, index })
            .Where(x => !string.IsNullOrWhiteSpace(x.header))
            .GroupBy(x => x.header)
            .ToDictionary(x => x.Key, x => x.First().index);

        foreach (var required in new[] { "NOMBRE", "CATEGORIA", "REFERENCIA", "PRECIO" })
        {
            if (!headerMap.ContainsKey(required)) throw new ValidationException($"La plantilla no contiene la columna obligatoria {required}.");
        }

        var categoryRows = await db.CategoriasProducto.ToListAsync(cancellationToken);
        var categories = categoryRows
            .GroupBy(x => NormalizeCategory(x.Nombre))
            .ToDictionary(x => x.Key, x => x.First());
        var productRows = await db.Productos.ToListAsync(cancellationToken);
        var products = productRows
            .GroupBy(x => x.Referencia.ToUpperInvariant())
            .ToDictionary(x => x.Key, x => x.First());

        var errors = new List<string>();
        var created = 0;
        var updated = 0;
        var lineNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line, separator);
            string Read(string key) => headerMap.TryGetValue(key, out var index) && index < values.Count ? values[index].Trim() : string.Empty;

            try
            {
                var name = Read("NOMBRE");
                var categoryName = NormalizeCategory(Read("CATEGORIA"));
                var reference = Read("REFERENCIA");
                if (string.IsNullOrWhiteSpace(name)) throw new ValidationException("Nombre es obligatorio.");
                if (string.IsNullOrWhiteSpace(categoryName)) throw new ValidationException("Categoria es obligatoria.");
                if (string.IsNullOrWhiteSpace(reference)) throw new ValidationException("Referencia es obligatoria.");

                if (!categories.TryGetValue(categoryName, out var category))
                {
                    category = new CategoriaProducto { Nombre = categoryName, Activa = true };
                    db.CategoriasProducto.Add(category);
                    categories[categoryName] = category;
                }

                var referenceKey = reference.ToUpperInvariant();
                var exists = products.TryGetValue(referenceKey, out var product);
                product ??= new Producto();

                product.Nombre = name;
                product.Categoria = category.Nombre;
                product.Marca = Read("MARCA");
                product.Modelo = Read("MODELO");
                product.Linea = NormalizeOptional(Read("LINEA"));
                product.Version = NormalizeOptional(Read("VERSION"));
                product.Referencia = reference;
                product.Descripcion = NormalizeOptional(Read("DESCRIPCION"));
                product.Cilindraje = ParseNullableInt(Read("CILINDRAJE"), "Cilindraje");
                product.Anio = ParseNullableInt(Read("ANO"), "Ano");
                product.Color = NormalizeOptional(Read("COLOR"));
                product.Precio = ParseMoney(Read("PRECIO"), "Precio");
                product.Soat = ParseMoneyOrDefault(Read("SOAT"), "SOAT");
                product.Matricula = ParseMoneyOrDefault(Read("MATRICULA"), "Matricula");
                product.Impuestos = ParseMoneyOrDefault(Read("IMPUESTOS"), "Impuestos");
                product.FichaTecnica = NormalizeOptional(Read("FICHATECNICA"));
                product.VigenteDesde = ParseNullableDate(Read("VIGENTEDESDE"), "VigenteDesde");
                product.Activo = ParseActive(Read("ACTIVO"));

                if (product.Precio <= 0) throw new ValidationException("Precio debe ser mayor a cero.");
                if (product.Soat < 0 || product.Matricula < 0 || product.Impuestos < 0) throw new ValidationException("Los cargos no pueden ser negativos.");

                if (exists)
                {
                    updated++;
                }
                else
                {
                    db.Productos.Add(product);
                    products[referenceKey] = product;
                    created++;
                }
            }
            catch (Exception ex) when (ex is ValidationException or FormatException)
            {
                errors.Add($"Fila {lineNumber}: {ex.Message}");
            }
        }

        if (created + updated > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return Ok(new { created, updated, errors, totalErrors = errors.Count });
    }

    [HttpPost("sync-external-inventory")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProductInventorySyncResultDto>> SyncExternalInventory(CancellationToken cancellationToken)
    {
        var inventoryConfig = await GetCurrentUserInventoryConfigAsync(cancellationToken);
        var warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(inventoryConfig.DatabaseName))
        {
            return BadRequest(new { detail = "Configure primero la base de datos de inventario en la empresa." });
        }

        if (inventoryConfig.AllowedWarehouses.Count == 0)
        {
            return BadRequest(new { detail = "Configure bodegas de inventario en la sede asignada al usuario o en las sedes activas de la empresa." });
        }

        var externalProducts = await ReadExternalInventoryProductsAsync(inventoryConfig, cancellationToken);
        if (externalProducts.Count == 0)
        {
            return Ok(new ProductInventorySyncResultDto(0, 0, 0, 0, ["No se encontraron productos con existencia en las bodegas configuradas."]));
        }

        var references = externalProducts.Select(x => x.Code).ToArray();
        var existingReferences = await db.Productos
            .Where(x => references.Contains(x.Referencia))
            .Select(x => x.Referencia)
            .ToListAsync(cancellationToken);
        var existingSet = existingReferences.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var category = await EnsureExternalInventoryCategoryAsync(cancellationToken);
        var created = 0;
        var existing = 0;
        var skipped = 0;
        foreach (var item in externalProducts)
        {
            if (string.IsNullOrWhiteSpace(item.Code))
            {
                skipped++;
                continue;
            }

            if (existingSet.Contains(item.Code))
            {
                existing++;
                continue;
            }

            var product = new Producto
            {
                Nombre = item.Name,
                Categoria = category.Nombre,
                Marca = GuessBrand(item.Name),
                Modelo = item.Name,
                Referencia = item.Code,
                Descripcion = NormalizeOptional(item.Presentation),
                Color = NormalizeOptional(item.Presentation),
                Precio = 0,
                Soat = 0,
                Matricula = 0,
                Impuestos = 0,
                FichaTecnica = BuildExternalInventoryTechnicalSheet(item),
                Activo = false
            };
            db.Productos.Add(product);
            existingSet.Add(item.Code);
            created++;
        }

        if (created > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (created >= MaxExternalInventorySyncRows)
        {
            warnings.Add("Se alcanzo el limite de sincronizacion. Si faltan productos, revise filtros o bodegas configuradas.");
        }

        return Ok(new ProductInventorySyncResultDto(created, existing, skipped, created, warnings));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> Create(UpsertProductDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var category = await ResolveCategoryAsync(dto.Category, cancellationToken);
        var product = new Producto
        {
            Nombre = dto.Name.Trim(),
            Categoria = category.Nombre,
            Marca = dto.Brand.Trim(),
            Modelo = dto.Model.Trim(),
            Linea = NormalizeOptional(dto.Line),
            Version = NormalizeOptional(dto.Version),
            Referencia = dto.Reference.Trim(),
            Descripcion = NormalizeOptional(dto.Description),
            Cilindraje = dto.EngineCc,
            Anio = dto.Year,
            Color = NormalizeOptional(dto.Color),
            Precio = dto.Price,
            Soat = dto.Soat,
            Matricula = dto.RegistrationFee,
            Impuestos = dto.Taxes,
            FichaTecnica = NormalizeOptional(dto.TechnicalSheet),
            VigenteDesde = dto.PriceValidFrom,
            Activo = dto.Active
        };
        db.Productos.Add(product);
        await ApplySalesPointPricesAsync(product, dto, category.Nombre, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await db.Entry(product).Collection(x => x.PreciosPorSede).Query().Include(x => x.PuntoVenta).LoadAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpsertProductDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var category = await ResolveCategoryAsync(dto.Category, cancellationToken);
        var product = await db.Productos
            .Include(x => x.Fotos)
            .Include(x => x.PreciosPorSede).ThenInclude(x => x.PuntoVenta)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");
        product.Nombre = dto.Name.Trim();
        product.Categoria = category.Nombre;
        product.Marca = dto.Brand.Trim();
        product.Modelo = dto.Model.Trim();
        product.Linea = NormalizeOptional(dto.Line);
        product.Version = NormalizeOptional(dto.Version);
        product.Referencia = dto.Reference.Trim();
        product.Descripcion = NormalizeOptional(dto.Description);
        product.Cilindraje = dto.EngineCc;
        product.Anio = dto.Year;
        product.Color = NormalizeOptional(dto.Color);
        product.Precio = dto.Price;
        product.Soat = dto.Soat;
        product.Matricula = dto.RegistrationFee;
        product.Impuestos = dto.Taxes;
        product.FichaTecnica = NormalizeOptional(dto.TechnicalSheet);
        product.VigenteDesde = dto.PriceValidFrom;
        product.Activo = dto.Active;
        await ApplySalesPointPricesAsync(product, dto, category.Nombre, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Productos
            .Include(x => x.Fotos)
            .Include(x => x.PreciosPorSede).ThenInclude(x => x.PuntoVenta)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");
        product.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    [HttpPost("{id:guid}/photos")]
    [Authorize(Roles = "Administrador,Supervisor")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<ProductDto>> UploadPhotos(Guid id, [FromForm] List<IFormFile> files, CancellationToken cancellationToken)
    {
        var product = await db.Productos
            .Include(x => x.Fotos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");

        if (files.Count == 0) throw new ValidationException("Debe seleccionar al menos una foto.");
        var nextOrder = product.Fotos.Count == 0 ? 1 : product.Fotos.Max(x => x.Orden) + 1;
        var hasDefault = product.Fotos.Any(x => x.EsPrincipalCotizacion);

        foreach (var file in files)
        {
            ValidatePhoto(file);
            await using var stream = file.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            var photo = new ProductoFoto
            {
                ProductoId = product.Id,
                NombreArchivo = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                TamanoBytes = file.Length,
                Datos = memory.ToArray(),
                EsPrincipalCotizacion = !hasDefault,
                Orden = nextOrder++
            };
            hasDefault = true;
            db.ProductoFotos.Add(photo);
        }

        await db.SaveChangesAsync(cancellationToken);
        await db.Entry(product).Collection(x => x.Fotos).LoadAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    [HttpPut("{id:guid}/photos/{photoId:guid}/quote-default")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> SetQuoteDefaultPhoto(Guid id, Guid photoId, CancellationToken cancellationToken)
    {
        var product = await db.Productos
            .Include(x => x.Fotos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");

        if (!product.Fotos.Any(x => x.Id == photoId)) throw new KeyNotFoundException("Foto no encontrada.");
        foreach (var photo in product.Fotos) photo.EsPrincipalCotizacion = photo.Id == photoId;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    [HttpDelete("{id:guid}/photos/{photoId:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> DeletePhoto(Guid id, Guid photoId, CancellationToken cancellationToken)
    {
        var product = await db.Productos
            .Include(x => x.Fotos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");
        var photo = product.Fotos.FirstOrDefault(x => x.Id == photoId) ?? throw new KeyNotFoundException("Foto no encontrada.");
        var wasDefault = photo.EsPrincipalCotizacion;
        db.ProductoFotos.Remove(photo);
        if (wasDefault)
        {
            var next = product.Fotos.Where(x => x.Id != photoId).OrderBy(x => x.Orden).FirstOrDefault();
            if (next is not null) next.EsPrincipalCotizacion = true;
        }

        await db.SaveChangesAsync(cancellationToken);
        await db.Entry(product).Collection(x => x.Fotos).LoadAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    private static ProductDto ToDto(Producto x) => new(
        x.Id,
        x.Nombre,
        x.Categoria,
        x.Marca,
        x.Modelo,
        x.Linea,
        x.Version,
        x.Referencia,
        x.Descripcion,
        x.Cilindraje,
        x.Anio,
        x.Color,
        x.Precio,
        x.Soat,
        x.Matricula,
        x.Impuestos,
        x.FichaTecnica,
        x.VigenteDesde,
        x.Activo,
        x.Fotos
            .OrderByDescending(photo => photo.EsPrincipalCotizacion)
            .ThenBy(photo => photo.Orden)
            .Select(photo => new ProductPhotoDto(
                photo.Id,
                photo.NombreArchivo,
                photo.ContentType,
                photo.TamanoBytes,
                photo.EsPrincipalCotizacion,
                $"data:{photo.ContentType};base64,{Convert.ToBase64String(photo.Datos)}"))
            .ToList(),
        x.PreciosPorSede
            .OrderBy(price => price.PuntoVenta == null ? string.Empty : price.PuntoVenta.Nombre)
            .Select(price => new ProductSalesPointPriceDto(
                price.PuntoVentaId,
                price.PuntoVenta?.Nombre,
                price.Precio,
                price.VigenteDesde,
                price.Activo))
            .ToList());

    private static void Validate(UpsertProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationException("El nombre del producto es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Category)) throw new ValidationException("La categoria es obligatoria.");
        if (string.IsNullOrWhiteSpace(dto.Reference)) throw new ValidationException("La referencia es obligatoria.");
        if (dto.Price <= 0) throw new ValidationException("El precio debe ser mayor a cero.");
        if (dto.Soat < 0) throw new ValidationException("El SOAT no puede ser negativo.");
        if (dto.RegistrationFee < 0) throw new ValidationException("La matricula no puede ser negativa.");
        if (dto.Taxes < 0) throw new ValidationException("Los impuestos no pueden ser negativos.");
        if (dto.SalesPointPrices is null) return;
        var duplicated = dto.SalesPointPrices
            .Where(x => x.SalesPointId != Guid.Empty)
            .GroupBy(x => x.SalesPointId)
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicated is not null) throw new ValidationException("No puede repetir una sede en los precios del producto.");
        foreach (var price in dto.SalesPointPrices)
        {
            if (price.SalesPointId == Guid.Empty) throw new ValidationException("Debe seleccionar una sede valida para el precio por sede.");
            if (price.Price <= 0) throw new ValidationException("El precio por sede debe ser mayor a cero.");
        }
    }

    private async Task ApplySalesPointPricesAsync(Producto product, UpsertProductDto dto, string categoryName, CancellationToken cancellationToken)
    {
        if (!IsApplianceCategory(categoryName))
        {
            db.ProductoPreciosSede.RemoveRange(product.PreciosPorSede);
            return;
        }

        var requestedPrices = (dto.SalesPointPrices ?? [])
            .Where(x => x.Active || x.Price > 0)
            .ToList();
        if (requestedPrices.Count == 0)
        {
            db.ProductoPreciosSede.RemoveRange(product.PreciosPorSede);
            return;
        }

        var salesPointIds = requestedPrices.Select(x => x.SalesPointId).Distinct().ToArray();
        var existingSalesPointIds = await db.PuntosVenta
            .Where(x => salesPointIds.Contains(x.Id) && x.Activa)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        if (existingSalesPointIds.Count != salesPointIds.Length) throw new ValidationException("Una de las sedes de precio no existe o esta inactiva.");

        foreach (var current in product.PreciosPorSede.Where(x => !salesPointIds.Contains(x.PuntoVentaId)).ToList())
        {
            db.ProductoPreciosSede.Remove(current);
        }

        foreach (var price in requestedPrices)
        {
            var current = product.PreciosPorSede.FirstOrDefault(x => x.PuntoVentaId == price.SalesPointId);
            if (current is null)
            {
                current = new ProductoPrecioSede
                {
                    Producto = product,
                    ProductoId = product.Id,
                    PuntoVentaId = price.SalesPointId
                };
                product.PreciosPorSede.Add(current);
            }

            current.Precio = price.Price;
            current.VigenteDesde = price.PriceValidFrom;
            current.Activo = price.Active;
        }
    }

    private static void ValidatePhoto(IFormFile file)
    {
        if (file.Length <= 0) throw new ValidationException("La foto esta vacia.");
        if (file.Length > 5_000_000) throw new ValidationException("Cada foto debe pesar maximo 5 MB.");
        var allowed = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException("Solo se permiten imagenes JPG, PNG o WebP.");
        }
    }

    private async Task<CategoriaProducto> ResolveCategoryAsync(string categoryName, CancellationToken cancellationToken)
    {
        var normalized = NormalizeCategory(categoryName);
        return await db.CategoriasProducto.FirstOrDefaultAsync(x => x.Nombre == normalized && x.Activa, cancellationToken)
            ?? throw new ValidationException("La categoria no existe o esta inactiva. Creela primero en Configuracion.");
    }

    private async Task<CategoriaProducto> EnsureExternalInventoryCategoryAsync(CancellationToken cancellationToken)
    {
        var category = await db.CategoriasProducto.FirstOrDefaultAsync(x => x.Nombre == ExternalInventoryCategory, cancellationToken);
        if (category is not null)
        {
            if (!category.Activa)
            {
                category.Activa = true;
            }

            return category;
        }

        category = new CategoriaProducto
        {
            Nombre = ExternalInventoryCategory,
            Descripcion = "Productos creados automaticamente desde el inventario externo.",
            CotizarComoPaquete = false,
            Activa = true
        };
        db.CategoriasProducto.Add(category);
        return category;
    }

    private async Task<ExternalInventoryProductConfig> GetCurrentUserInventoryConfigAsync(CancellationToken cancellationToken)
    {
        if (tenantContext.EmpresaId is not Guid companyId)
        {
            return new ExternalInventoryProductConfig(null, []);
        }

        var databaseName = await db.Empresas
            .IgnoreQueryFilters()
            .Where(x => x.Id == companyId)
            .Select(x => x.BaseDatosInventarioExterno)
            .FirstOrDefaultAsync(cancellationToken);

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return new ExternalInventoryProductConfig(NormalizeDatabaseName(databaseName), []);
        }

        var userSalesPointWarehouses = await db.Usuarios
            .Where(x => x.Id == userId && x.EmpresaId == companyId && x.PuntoVentaId.HasValue && x.PuntoVenta!.Activa)
            .Select(x => x.PuntoVenta!.BodegasInventarioExterno)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(userSalesPointWarehouses))
        {
            return new ExternalInventoryProductConfig(NormalizeDatabaseName(databaseName), ParseWarehouseCodes(userSalesPointWarehouses));
        }

        var warehouseValues = User.IsInRole("Supervisor")
            ? await db.UsuariosSedesSupervisadas
                .Where(x => x.UsuarioId == userId && x.EmpresaId == companyId && x.PuntoVenta != null && x.PuntoVenta.Activa)
                .Select(x => x.PuntoVenta!.BodegasInventarioExterno)
                .ToListAsync(cancellationToken)
            : User.IsInRole("Administrador")
                ? await db.PuntosVenta
                    .Where(x => x.EmpresaId == companyId && x.Activa && x.BodegasInventarioExterno != null && x.BodegasInventarioExterno != "")
                    .Select(x => x.BodegasInventarioExterno)
                    .ToListAsync(cancellationToken)
                : [];

        return new ExternalInventoryProductConfig(NormalizeDatabaseName(databaseName), ParseWarehouseCodes(warehouseValues));
    }

    private async Task<IReadOnlyCollection<ExternalInventoryProductRow>> ReadExternalInventoryProductsAsync(ExternalInventoryProductConfig inventoryConfig, CancellationToken cancellationToken)
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

        var warehouseParameters = inventoryConfig.AllowedWarehouses.Select((_, index) => $"@AllowedWarehouse{index}").ToArray();
        var rows = new List<ExternalInventoryProductRow>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"""
            SELECT TOP (@Take)
                Codigo,
                MAX(Nombre) AS Nombre,
                MAX(Presentacion) AS Presentacion,
                SUM(ISNULL(Existencias, 0)) AS Existencias
            FROM {BuildInventoryViewName(inventoryConfig.DatabaseName)}
            WHERE Codigo IS NOT NULL
              AND LTRIM(RTRIM(Codigo)) <> ''
              AND ISNULL(Existencias, 0) > 0
              AND Bodega IN ({string.Join(", ", warehouseParameters)})
            GROUP BY Codigo
            ORDER BY MAX(Nombre), Codigo;
            """;
        command.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = MaxExternalInventorySyncRows });
        foreach (var (allowedWarehouse, index) in inventoryConfig.AllowedWarehouses.Select((value, index) => (value, index)))
        {
            command.Parameters.Add(new SqlParameter($"@AllowedWarehouse{index}", SqlDbType.VarChar, 40) { Value = allowedWarehouse });
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var code = ReadString(reader, "Codigo");
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }

            var name = ReadString(reader, "Nombre");
            rows.Add(new ExternalInventoryProductRow(
                code,
                string.IsNullOrWhiteSpace(name) ? code : name,
                ReadNullableString(reader, "Presentacion"),
                reader["Existencias"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Existencias"])));
        }

        return rows;
    }

    private static string BuildInventoryViewName(string? databaseName)
    {
        var normalizedDatabase = NormalizeDatabaseName(databaseName);
        if (string.IsNullOrWhiteSpace(normalizedDatabase))
        {
            throw new InvalidOperationException("No hay base de datos de inventario configurada para la empresa.");
        }

        return $"[{normalizedDatabase}].[{InventorySchema}].[{InventoryView}]";
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

    private static IReadOnlyCollection<string> ParseWarehouseCodes(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value
            .Split([',', ';', '|', '\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static IReadOnlyCollection<string> ParseWarehouseCodes(IReadOnlyCollection<string?> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => value!.Split([',', ';', '|', '\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(x => x.ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string ReadString(IDataRecord reader, string name) =>
        reader[name] == DBNull.Value ? string.Empty : Convert.ToString(reader[name])?.Trim() ?? string.Empty;

    private static string? ReadNullableString(IDataRecord reader, string name)
    {
        var value = ReadString(reader, name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string GuessBrand(string value)
    {
        var first = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(first) ? "Inventario" : first;
    }

    private static string? BuildExternalInventoryTechnicalSheet(ExternalInventoryProductRow item)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(item.Presentation)) parts.Add($"Presentacion: {item.Presentation}");
        parts.Add($"Existencia al sincronizar: {item.Quantity}");
        return string.Join(Environment.NewLine, parts);
    }

    private static string NormalizeCategory(string category) => string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();

    private static bool IsApplianceCategory(string category) =>
        NormalizeCategory(category).Contains("electrodom", StringComparison.OrdinalIgnoreCase);

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static char DetectSeparator(string line) =>
        line.Count(x => x == ';') >= line.Count(x => x == ',') ? ';' : ',';

    private static List<string> ParseCsvLine(string line, char separator)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (character == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == separator && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }
        }

        values.Add(current.ToString());
        return values;
    }

    private static string NormalizeHeader(string value)
    {
        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(character)) builder.Append(char.ToUpperInvariant(character));
        }

        return builder.ToString();
    }

    private static decimal ParseMoney(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ValidationException($"{field} es obligatorio.");
        var normalized = value.Replace("$", string.Empty).Replace(" ", string.Empty).Trim();
        if (normalized.Contains('.') && normalized.Contains(','))
        {
            normalized = normalized.Replace(".", string.Empty).Replace(",", ".");
        }
        else if (normalized.Count(x => x == '.') > 1)
        {
            normalized = normalized.Replace(".", string.Empty);
        }
        else if (normalized.Contains(','))
        {
            normalized = normalized.Replace(",", ".");
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw new ValidationException($"{field} no tiene un valor numerico valido.");
    }

    private static decimal ParseMoneyOrDefault(string value, string field) =>
        string.IsNullOrWhiteSpace(value) ? 0 : ParseMoney(value, field);

    private static int? ParseNullableInt(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw new ValidationException($"{field} no tiene un numero valido.");
    }

    private static DateTime? ParseNullableDate(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" };
        return DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? result
            : throw new ValidationException($"{field} debe tener formato yyyy-MM-dd o dd/MM/yyyy.");
    }

    private static bool ParseActive(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        var normalized = value.Trim().ToUpperInvariant();
        return normalized is "SI" or "S" or "TRUE" or "1" or "ACTIVO" or "ACTIVA";
    }

    private sealed record ExternalInventoryProductConfig(string? DatabaseName, IReadOnlyCollection<string> AllowedWarehouses);
    private sealed record ExternalInventoryProductRow(string Code, string Name, string? Presentation, int Quantity);
}
