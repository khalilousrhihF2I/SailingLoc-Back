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
        // Brand palette (kept in sync with SimpleTemplateRenderer)
        const string brandPrimary   = "#0369a1";
        const string brandPrimaryDk = "#075985";
        const string brandAccent    = "#0ea5e9";
        const string textPrimary    = "#0f172a";
        const string textMuted      = "#475569";
        const string textSubtle     = "#94a3b8";
        const string divider        = "#e2e8f0";
        const string pageBg         = "#f1f5f9";

        var safeName = System.Net.WebUtility.HtmlEncode(firstName ?? string.Empty);
        var safeUrl  = (resetUrl ?? string.Empty).Replace("\"", "&quot;");
        var visibleUrl = System.Net.WebUtility.HtmlEncode(resetUrl ?? string.Empty);
        var preheader = "Réinitialisez votre mot de passe SailingLoc (lien valable 1 heure).";

        return $@"<!doctype html>
<html lang=""fr"">
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
    <meta name=""x-apple-disable-message-reformatting"">
    <title>Réinitialisation de votre mot de passe</title>
  </head>
  <body style=""margin:0;padding:0;background:{pageBg};font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:{textPrimary};"">
    <div style=""display:none;max-height:0;overflow:hidden;opacity:0;visibility:hidden;mso-hide:all;font-size:1px;line-height:1px;color:{pageBg};"">{preheader}</div>
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:{pageBg};"">
      <tr>
        <td align=""center"" style=""padding:32px 16px;"">
          <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:600px;width:100%;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 16px rgba(15,23,42,0.06);"">
            <tr>
              <td style=""background:linear-gradient(135deg,{brandPrimary} 0%,{brandPrimaryDk} 100%);background-color:{brandPrimary};padding:28px 32px;color:#ffffff;"">
                <div style=""font-size:13px;letter-spacing:2px;text-transform:uppercase;color:#bae6fd;font-weight:600;"">⚓ SailingLoc</div>
                <div style=""padding-top:10px;font-size:22px;font-weight:700;line-height:28px;color:#ffffff;"">Réinitialisation du mot de passe</div>
                <div style=""margin-top:6px;color:#e0f2fe;font-size:14px;line-height:20px;"">Un lien sécurisé vous est envoyé</div>
              </td>
            </tr>
            <tr>
              <td style=""padding:32px;"">
                <p style=""margin:0 0 16px 0;font-size:16px;line-height:24px;color:{textPrimary};"">Bonjour {safeName},</p>
                <p style=""margin:0 0 14px 0;font-size:15px;line-height:22px;color:{textMuted};"">Vous avez demandé la réinitialisation de votre mot de passe. Cliquez sur le bouton ci-dessous pour définir un nouveau mot de passe.</p>

                <div style=""text-align:center;margin:28px 0;"">
                  <!--[if mso]>
                  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{safeUrl}"" style=""height:48px;v-text-anchor:middle;width:280px;"" arcsize=""20%"" stroke=""f"" fillcolor=""{brandPrimary}"">
                    <w:anchorlock/>
                    <center style=""color:#ffffff;font-family:sans-serif;font-size:16px;font-weight:bold;"">Réinitialiser mon mot de passe</center>
                  </v:roundrect>
                  <![endif]-->
                  <!--[if !mso]><!-- -->
                  <a href=""{safeUrl}"" target=""_blank"" style=""display:inline-block;background:{brandPrimary};background-image:linear-gradient(135deg,{brandAccent} 0%,{brandPrimary} 100%);color:#ffffff;text-decoration:none;font-weight:600;font-size:15px;padding:14px 32px;border-radius:10px;letter-spacing:0.2px;box-shadow:0 4px 10px rgba(3,105,161,0.25);"">Réinitialiser mon mot de passe</a>
                  <!--<![endif]-->
                </div>

                <div style=""margin:20px 0;padding:14px 18px;background:#fff7ed;border-left:4px solid #f59e0b;border-radius:8px;font-size:14px;line-height:20px;color:#7c2d12;"">
                  ⏱ Ce lien expire dans <strong>1 heure</strong> pour votre sécurité.
                </div>

                <p style=""margin:14px 0 0 0;font-size:13px;line-height:20px;color:{textSubtle};"">Si vous n'êtes pas à l'origine de cette demande, ignorez simplement cet email — votre mot de passe restera inchangé.</p>

                <hr style=""margin:24px 0;border:none;border-top:1px solid {divider};"">
                <p style=""margin:0;font-size:12px;line-height:18px;color:{textSubtle};"">Si le bouton ne fonctionne pas, copiez et collez ce lien dans votre navigateur :<br/>
                  <a href=""{safeUrl}"" style=""color:{brandPrimary};word-break:break-all;text-decoration:none;"">{visibleUrl}</a>
                </p>
              </td>
            </tr>
            <tr>
              <td style=""padding:20px 32px;border-top:1px solid {divider};background:#fafafa;"">
                <div style=""font-size:12px;line-height:18px;color:{textSubtle};text-align:center;"">© {DateTime.UtcNow.Year} SailingLoc — Tous droits réservés.</div>
              </td>
            </tr>
          </table>
          <div style=""font-size:11px;color:{textSubtle};padding:12px;max-width:600px;line-height:16px;"">
            Vous recevez cet email parce qu'une réinitialisation de mot de passe a été demandée pour votre compte SailingLoc.
          </div>
        </td>
      </tr>
    </table>
  </body>
</html>";
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