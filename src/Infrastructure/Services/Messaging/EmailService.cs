using Core.Entities;
using Core.Interfaces.Notifications;
using Core.Interfaces.Services.Notifications;
using Core.Models.Templates;
using Microsoft.Extensions.Configuration;
using Resend;
using System.Linq;

namespace Infrastructure.Services.Messaging;

public class EmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;
    private readonly ITemplateRenderer _renderer;

    public EmailService(IConfiguration configuration, ITemplateRenderer renderer)
    {
        _configuration = configuration;

        var apiKey = Environment.GetEnvironmentVariable("RESEND_KEY")
                    ?? _configuration["Resend:dataFlow"]
                    ?? throw new Exception("Missing RESEND_KEY environment variable");

        _resend = ResendClient.Create(apiKey);
        _renderer = renderer;
    }


    public async Task<bool> SendPasswordResetEmailAsync(AppUser user, string resetToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var resetUrl = _configuration["Resend:ResetPasswordUrl"] ?? "http://localhost:3000/reset-password";
            var fullResetUrl = $"{resetUrl}?token={Uri.EscapeDataString(resetToken)}";

            var email = user.Email;
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(user.Email), "L'email de l'utilisateur ne peut pas être null ou vide.");

            var emailMessage = new EmailMessage
            {
                From = EmailAddress.Parse(_configuration["Resend:FromEmail"] ?? "support@sailingLoc.app"),
                To = EmailAddressList.From(email),
                Subject = "Réinitialisation de votre mot de passe",
                HtmlBody = GeneratePasswordResetEmailTemplate(user.FirstName, fullResetUrl)
            };

            var response = await _resend.EmailSendAsync(emailMessage, cancellationToken);

            // Vérifier si la réponse indique le succès
            return response.Success;
        }
        catch (Exception ex)
        {
            // Log l'exception
            Console.WriteLine($"Erreur lors de l'envoi de l'email à {user.Email}: {ex.Message}");
            return false;
        }
    }

    private static string GeneratePasswordResetEmailTemplate(string firstName, string resetUrl)
    {
        return $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                <h2>Réinitialisation de votre mot de passe</h2>
                <p>Bonjour {firstName},</p>
                <p>Vous avez demandé la réinitialisation de votre mot de passe. Cliquez sur le lien ci-dessous pour procéder :</p>
                <p style=""margin: 20px 0;"">
                    <a href=""{resetUrl}"" style=""background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">
                        Réinitialiser mon mot de passe
                    </a>
                </p>
                <p><strong>Ce lien expire dans 1 heure.</strong></p>
                <p>Si vous n'avez pas demandé cette réinitialisation, ignorez simplement cet email.</p>
                <hr style=""margin: 30px 0; border: none; border-top: 1px solid #eee;"">
                <p style=""color: #666; font-size: 12px;"">
                    Si le bouton ne fonctionne pas, copiez et collez ce lien dans votre navigateur :<br>
                    {resetUrl}
                </p>
            </div>";
    }

    // Helper to send a rendered template to one or more recipients
    private async Task<bool> SendRenderedEmailAsync(IEnumerable<string> recipients, (string Subject, string HtmlBody, string TextBody) rendered, CancellationToken cancellationToken)
    {
        if (recipients == null || !recipients.Any()) return false;

        var emailMessage = new EmailMessage
        {
            From = EmailAddress.Parse(_configuration["Resend:FromEmail"] ?? "support@sailingloc.app"),
            To = EmailAddressList.From(recipients.ToArray()),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            TextBody = rendered.TextBody
        };

        var response = await _resend.EmailSendAsync(emailMessage, cancellationToken);
        return response.Success;
    }

    public async Task<bool> SendNewUserCreatedEmailAsync(IEnumerable<string> recipients, NewUserTemplateModel model, CancellationToken cancellationToken = default)
    {
        var rendered = _renderer.RenderNewUserCreatedEmail(model);
        return await SendRenderedEmailAsync(recipients, rendered, cancellationToken);
    }

    public async Task<bool> SendReservationCreatedEmailAsync(IEnumerable<string> recipients, ReservationTemplateModel model, CancellationToken cancellationToken = default)
    {
        var rendered = _renderer.RenderReservationCreatedEmail(model);
        return await SendRenderedEmailAsync(recipients, rendered, cancellationToken);
    }

    public async Task<bool> SendContactMessageEmailAsync(IEnumerable<string> recipients, ContactMessageTemplateModel model, CancellationToken cancellationToken = default)
    {
        var rendered = _renderer.RenderContactMessageEmail(model);
        return await SendRenderedEmailAsync(recipients, rendered, cancellationToken);
    }


    public async Task<bool> SendCancellationRequestEmailAsync(IEnumerable<string> recipients, CancellationRequestTemplateModel model, CancellationToken cancellationToken = default)
    {
        var rendered = _renderer.RenderCancellationRequestEmail(model);
        return await SendRenderedEmailAsync(recipients, rendered, cancellationToken);
    }

    public async Task<bool> SendDocumentUploadedEmailAsync(IEnumerable<string> recipients, DocumentUploadedTemplateModel model, CancellationToken cancellationToken = default)
    {
        var rendered = _renderer.RenderDocumentUploadedEmail(model);
        return await SendRenderedEmailAsync(recipients, rendered, cancellationToken);
    }

    public async Task<bool> SendReservationApprovedEmailAsync(IEnumerable<string> recipients, ReservationApprovedTemplateModel model, CancellationToken cancellationToken = default)
    {
        var rendered = _renderer.RenderReservationApprovedEmail(model);
        return await SendRenderedEmailAsync(recipients, rendered, cancellationToken);
    }

    public async Task<bool> SendBoatApprovedEmailAsync(IEnumerable<string> recipients, BoatApprovedTemplateModel model, CancellationToken cancellationToken = default)
    {
        var rendered = _renderer.RenderBoatApprovedEmail(model);
        return await SendRenderedEmailAsync(recipients, rendered, cancellationToken);
    }
}