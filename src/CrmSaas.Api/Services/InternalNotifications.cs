using System.Collections.Concurrent;
using System.Threading.Channels;
using CrmSaas.Api.Authorization;
using CrmSaas.Application.Abstractions;
using CrmSaas.Domain.Common;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Services;

public sealed class NotificationBroadcaster
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<Guid>>> subscribers = new();

    public (Guid SubscriptionId, ChannelReader<Guid> Reader) Subscribe(Guid userId)
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        subscribers.GetOrAdd(userId, _ => new()).TryAdd(id, channel);
        return (id, channel.Reader);
    }

    public void Unsubscribe(Guid userId, Guid subscriptionId)
    {
        if (!subscribers.TryGetValue(userId, out var listeners)) return;
        if (listeners.TryRemove(subscriptionId, out var channel)) channel.Writer.TryComplete();
    }

    public void Notify(IEnumerable<Guid> userIds, Guid applicationId)
    {
        foreach (var userId in userIds.Distinct())
            if (subscribers.TryGetValue(userId, out var listeners))
                foreach (var channel in listeners.Values) channel.Writer.TryWrite(applicationId);
    }
}

public sealed class InternalNotificationService(CrmDbContext db, ITenantContext tenantContext, NotificationBroadcaster broadcaster)
{
    public async Task PublishApplicationChangedAsync(SolicitudCredito application, CancellationToken cancellationToken)
    {
        var sellerEmails = await SellerEmailsAsync(application, cancellationToken);
        var recipients = await db.Usuarios
            .Where(user => user.Activo && (sellerEmails.Contains(user.Email) || user.UsuarioRoles.Any(role =>
                role.Rol != null && (role.Rol.Nombre == "Administrador" || role.Rol.Nombre == "Supervisor" || role.Rol.Nombre == CreditRoles.Approver))))
            .Select(user => user.Id).ToListAsync(cancellationToken);
        broadcaster.Notify(recipients, application.Id);
    }

    public Task<IReadOnlyList<Guid>> QueueForManagersAsync(SolicitudCredito application, string type,
        string title, string message, CancellationToken cancellationToken) =>
        QueueAsync(application, type, title, message, includeApprovers: true, deduplicate: true, cancellationToken: cancellationToken);

    public Task<IReadOnlyList<Guid>> QueueForPostApprovalManagersAsync(SolicitudCredito application, string type,
        string title, string message, CancellationToken cancellationToken) =>
        QueueAsync(application, type, title, message, includeApprovers: false, deduplicate: false, cancellationToken: cancellationToken);

    public Task<IReadOnlyList<Guid>> QueueForCreditReviewAsync(SolicitudCredito application, string type,
        string title, string message, CancellationToken cancellationToken) =>
        QueueAsync(application, type, title, message, includeApprovers: true, deduplicate: false, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<Guid>> QueueForCreatorAsync(SolicitudCredito application, string type,
        string title, string message, CancellationToken cancellationToken)
    {
        var sellerEmails = await SellerEmailsAsync(application, cancellationToken);
        var recipients = await db.Usuarios
            .Where(user => user.Activo && sellerEmails.Contains(user.Email))
            .Select(user => user.Id).ToListAsync(cancellationToken);
        return await QueueForRecipientsAsync(application, recipients, type, title, message, deduplicate: false, cancellationToken);
    }

    private async Task<string[]> SellerEmailsAsync(SolicitudCredito application, CancellationToken cancellationToken)
    {
        var quoteSeller = application.CotizacionId.HasValue
            ? await db.Cotizaciones.Where(quote => quote.Id == application.CotizacionId.Value)
                .Select(quote => quote.UsuarioCreacion).FirstOrDefaultAsync(cancellationToken)
            : null;
        return new[] { application.UsuarioCreacion, quoteSeller }
            .Where(email => !string.IsNullOrWhiteSpace(email)).Select(email => email!).Distinct().ToArray();
    }

    private async Task<IReadOnlyList<Guid>> QueueAsync(SolicitudCredito application, string type,
        string title, string message, bool includeApprovers, bool deduplicate, CancellationToken cancellationToken)
    {
        var recipients = await db.Usuarios
            .Where(user => user.Activo && user.UsuarioRoles.Any(role => role.Rol != null &&
                (role.Rol.Nombre == "Administrador" || role.Rol.Nombre == "Supervisor" || (includeApprovers && role.Rol.Nombre == CreditRoles.Approver))))
            .Select(user => user.Id).ToListAsync(cancellationToken);
        return await QueueForRecipientsAsync(application, recipients, type, title, message, deduplicate, cancellationToken);
    }

    private async Task<IReadOnlyList<Guid>> QueueForRecipientsAsync(SolicitudCredito application, IReadOnlyList<Guid> recipients,
        string type, string title, string message, bool deduplicate, CancellationToken cancellationToken)
    {
        if (recipients.Count == 0) return recipients;
        if (message.Length > 500) message = message[..497] + "...";
        var companyId = tenantContext.EmpresaId ?? throw new InvalidOperationException("No se resolvió la empresa de la notificación.");
        if (application.EmpresaId != companyId) throw new InvalidOperationException("La solicitud no pertenece a la empresa activa.");

        var existing = await db.AlertasInternas
            .Where(alert => alert.SolicitudCreditoId == application.Id && alert.Tipo == type && recipients.Contains(alert.UsuarioId))
            .ToListAsync(cancellationToken);
        var existingByUser = existing.ToDictionary(alert => alert.UsuarioId);
        var newRecipients = recipients.Where(userId => !existingByUser.ContainsKey(userId)).Distinct().ToList();
        if (!deduplicate)
            foreach (var alert in existing)
            {
                alert.Titulo = title;
                alert.Mensaje = message;
                alert.FechaLectura = null;
                alert.FechaActualizacion = ColombiaTime.Now;
            }
        foreach (var userId in newRecipients)
            db.AlertasInternas.Add(new AlertaInterna
            {
                EmpresaId = companyId,
                UsuarioId = userId,
                SolicitudCreditoId = application.Id,
                Tipo = type,
                Titulo = title,
                Mensaje = message
            });
        return deduplicate ? newRecipients : recipients;
    }

}
