using System.Security.Claims;
using System.Text.Json;
using CrmSaas.Api.Services;
using CrmSaas.Domain.Common;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class InternalNotificationsController(CrmDbContext db, NotificationBroadcaster broadcaster) : ControllerBase
{
    private Guid CurrentUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new UnauthorizedAccessException("Usuario no identificado.");

    [HttpGet]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        var rows = await db.AlertasInternas.AsNoTracking()
            .Where(alert => alert.UsuarioId == userId)
            .OrderByDescending(alert => alert.FechaCreacion)
            .Take(80)
            .Select(alert => new NotificationDto(alert.Id, alert.Tipo, alert.Titulo, alert.Mensaje,
                alert.SolicitudCreditoId, alert.FechaCreacion, alert.FechaLectura))
            .ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPut("{id:guid}/read")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var alert = await db.AlertasInternas.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == CurrentUserId, cancellationToken);
        if (alert is null) return NotFound();
        alert.FechaLectura ??= ColombiaTime.Now;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("read-all")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var rows = await db.AlertasInternas.Where(x => x.UsuarioId == CurrentUserId && x.FechaLectura == null)
            .ToListAsync(cancellationToken);
        foreach (var row in rows) row.FechaLectura = ColombiaTime.Now;
        if (rows.Count > 0) await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("stream")]
    public async Task Stream(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache, no-transform";
        Response.Headers["X-Accel-Buffering"] = "no";
        var (subscriptionId, reader) = broadcaster.Subscribe(userId);
        try
        {
            await Response.WriteAsync("retry: 2000\n: connected\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                using var heartbeat = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                heartbeat.CancelAfter(TimeSpan.FromSeconds(20));
                Guid? changedApplicationId;
                try { changedApplicationId = await reader.ReadAsync(heartbeat.Token); }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { changedApplicationId = null; }
                var message = changedApplicationId.HasValue
                    ? $"event: refresh\ndata: {JsonSerializer.Serialize(new { applicationId = changedApplicationId.Value })}\n\n"
                    : ": heartbeat\n\n";
                await Response.WriteAsync(message, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally { broadcaster.Unsubscribe(userId, subscriptionId); }
    }

    public sealed record NotificationDto(Guid Id, string Type, string Title, string Message,
        Guid CreditApplicationId, DateTime CreatedAt, DateTime? ReadAt);
}
