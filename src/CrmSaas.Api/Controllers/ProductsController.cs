using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> Get(CancellationToken cancellationToken)
    {
        var products = await db.Productos
            .Include(x => x.Fotos)
            .OrderBy(x => x.Categoria).ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
        return Ok(products.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> Create(UpsertProductDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var product = new Producto
        {
            Nombre = dto.Name.Trim(),
            Categoria = NormalizeCategory(dto.Category),
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
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpsertProductDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var product = await db.Productos
            .Include(x => x.Fotos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");
        product.Nombre = dto.Name.Trim();
        product.Categoria = NormalizeCategory(dto.Category);
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
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Productos
            .Include(x => x.Fotos)
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

    private static string NormalizeCategory(string category) => string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
