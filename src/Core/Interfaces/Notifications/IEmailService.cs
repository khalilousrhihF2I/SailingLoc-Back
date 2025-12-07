using Core.Entities;
using Core.Models.Templates;

namespace Core.Interfaces.Notifications;

public interface IEmailService
{
    Task<bool> SendContactMessageEmailAsync(IEnumerable<string> recipients, ContactMessageTemplateModel model, CancellationToken cancellationToken = default);

    Task<bool> SendPasswordResetEmailAsync(AppUser user, string resetToken, CancellationToken cancellationToken = default);

    Task<bool> SendNewUserCreatedEmailAsync(IEnumerable<string> recipients, NewUserTemplateModel model, CancellationToken cancellationToken = default);

    Task<bool> SendReservationCreatedEmailAsync(IEnumerable<string> recipients, ReservationTemplateModel model, CancellationToken cancellationToken = default);

    Task<bool> SendCancellationRequestEmailAsync(IEnumerable<string> recipients, CancellationRequestTemplateModel model, CancellationToken cancellationToken = default);

    Task<bool> SendDocumentUploadedEmailAsync(IEnumerable<string> recipients, DocumentUploadedTemplateModel model, CancellationToken cancellationToken = default);

    Task<bool> SendReservationApprovedEmailAsync(IEnumerable<string> recipients, ReservationApprovedTemplateModel model, CancellationToken cancellationToken = default);

    Task<bool> SendBoatApprovedEmailAsync(IEnumerable<string> recipients, BoatApprovedTemplateModel model, CancellationToken cancellationToken = default);
}