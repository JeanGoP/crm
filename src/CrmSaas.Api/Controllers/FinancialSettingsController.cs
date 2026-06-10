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
[Route("api/financial-settings")]
public sealed class FinancialSettingsController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FinancialSettingsDto>> Get(CancellationToken cancellationToken)
    {
        var settings = await GetOrCreateAsync(cancellationToken);
        return Ok(ToDto(settings));
    }

    [HttpPut]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<FinancialSettingsDto>> Update(UpsertFinancialSettingsDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var settings = await GetOrCreateAsync(cancellationToken);
        settings.SalarioMinimoVigente = dto.MinimumWage;
        settings.TasaConsumoEa = dto.ConsumerAnnualRate;
        settings.TasaBajoMontoEa = dto.LowAmountAnnualRate;
        settings.TasaFactorMensual = dto.FactorMonthlyRate;
        settings.PlazoMaximoMeses = dto.MaxTermMonths;
        settings.RedondeoCuota = dto.PaymentRounding;
        settings.UsarTablaMontelibano = dto.UseMontelibanoTable;
        settings.Activa = dto.Active;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(settings));
    }

    private async Task<ConfiguracionFinancieraEmpresa> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var settings = await db.ConfiguracionesFinancierasEmpresa.FirstOrDefaultAsync(cancellationToken);
        if (settings is not null) return settings;

        settings = new ConfiguracionFinancieraEmpresa();
        db.ConfiguracionesFinancierasEmpresa.Add(settings);
        await db.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private static void Validate(UpsertFinancialSettingsDto dto)
    {
        if (dto.MinimumWage <= 0) throw new ValidationException("El salario minimo debe ser mayor a cero.");
        if (dto.ConsumerAnnualRate < 0) throw new ValidationException("La tasa de consumo no puede ser negativa.");
        if (dto.LowAmountAnnualRate < 0) throw new ValidationException("La tasa de bajo monto no puede ser negativa.");
        if (dto.FactorMonthlyRate < 0) throw new ValidationException("La tasa de factores no puede ser negativa.");
        if (dto.MaxTermMonths <= 0) throw new ValidationException("El plazo maximo debe ser mayor a cero.");
        if (dto.PaymentRounding <= 0) throw new ValidationException("El redondeo de cuota debe ser mayor a cero.");
    }

    private static FinancialSettingsDto ToDto(ConfiguracionFinancieraEmpresa settings) =>
        new(
            settings.Id,
            settings.SalarioMinimoVigente,
            settings.TasaConsumoEa,
            settings.TasaBajoMontoEa,
            settings.TasaFactorMensual,
            settings.PlazoMaximoMeses,
            settings.RedondeoCuota,
            settings.UsarTablaMontelibano,
            settings.Activa);
}
