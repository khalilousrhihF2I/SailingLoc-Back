using Core.Interfaces.Services.Notifications;
using Core.Models.Templates;
using Infrastructure.Models.Templates;
using System.Text;

namespace Infrastructure.Templates
{
    public class SimpleTemplateRenderer : ITemplateRenderer
    {
        // Helper to build consistent HTML envelope used by all templates
        private static string BuildHtmlEnvelope(string brandName, string subject, string recipientName, string innerHtml)
        {
            var greeting = string.IsNullOrWhiteSpace(recipientName) ? "Bonjour" : $"Bonjour {recipientName},";

            return $@"<!doctype html>
<html lang=""fr""> 
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>{subject}</title>
  </head>
  <body style=""font-family:Arial,Helvetica,sans-serif;line-height:1.5;color:#111;margin:0;padding:24px;background:#f7f7f8;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
      <tr><td align=""center"">
        <table role=""presentation"" width=""100%"" style=""max-width:560px;background:#ffffff;border-radius:12px;padding:24px;border:1px solid #eee;"">
          <tr>
            <td style=""font-size:18px;font-weight:600;"">{brandName}</td>
          </tr>
          <tr><td style=""padding-top:12px;color:#444;"">{greeting}</td></tr>
          <tr>
            <td style=""padding-top:8px;color:#444;"">
              {innerHtml}
            </td>
          </tr>
        </table>
        <div style=""color:#999;font-size:12px;padding:16px;"">© {DateTime.UtcNow:yyyy} {brandName}</div>
      </td></tr>
    </table>
  </body>
</html>";
        }

        // Helper to build consistent plain-text envelope used by all templates
        private static string BuildTextEnvelope(string recipientName, string innerText, string supportEmail = null)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(recipientName)) sb.AppendLine($"Bonjour {recipientName},");
            sb.AppendLine();
            sb.AppendLine(innerText);
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(supportEmail)) sb.AppendLine($"Support : {supportEmail}");
            return sb.ToString();
        }

        public (string Subject, string HtmlBody, string TextBody) RenderResetCodeEmail(ResetCodeTemplateModel m)
        {
            var subject = $"{m.BrandName} — Code de réinitialisation";

            var innerText = new StringBuilder()
                .AppendLine($"Votre code de réinitialisation : {m.Code}")
                .AppendLine($"Il expire dans {m.ExpiresMinutes} minutes.")
                .AppendLine()
                .AppendLine($"Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.")
                .ToString();

            // inner HTML contains the code box and explanation
            var innerHtml = $@"<div style=""padding-top:8px;color:#444;"">Utilisez ce code pour réinitialiser votre mot de passe :</div>
<div style=""padding:16px 0;"">
  <div style=""font-size:28px;font-weight:700;letter-spacing:6px;text-align:center;border:1px dashed #ddd;border-radius:10px;padding:16px;"">{m.Code}</div>
</div>
<div style=""color:#666;"">Le code expire dans <b>{m.ExpiresMinutes} minutes</b>.</div>
<div style=""padding-top:16px;color:#888;font-size:12px;"">Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer cet email.<br/>{(m.SupportEmail is null ? "" : $"Besoin d'aide ? {m.SupportEmail}")}</div>";

            var html = BuildHtmlEnvelope(m.BrandName, subject, m.RecipientName, innerHtml);
            var text = BuildTextEnvelope(m.RecipientName, innerText, m.SupportEmail);

            return (subject, html, text);
        }

        public string RenderResetCodeSms(ResetCodeTemplateModel m)
        {
            return $"{m.BrandName}: votre code est {m.Code}. Expire dans {m.ExpiresMinutes} min.";
        }

        // New user creation: notify admin
        public (string Subject, string HtmlBody, string TextBody) RenderNewUserCreatedEmail(NewUserTemplateModel m)
        {
            var subject = $"{m.BrandName} — Nouvel utilisateur enregistré";
            var innerText = $"Nouvel utilisateur: {m.UserName} ({m.UserEmail})\nCréé le: {m.CreatedAt:u}";
            var innerHtml = $"<p>Un nouvel utilisateur s'est enregistré: <strong>{m.UserName}</strong> ({m.UserEmail}).</p><p>Créé le: {m.CreatedAt:u}</p>";

            var html = BuildHtmlEnvelope(m.BrandName, subject, null, innerHtml);
            var text = BuildTextEnvelope(null, innerText);
            return (subject, html, text);
        }

        // Reservation created: notify renter, owner and admin
        public (string Subject, string HtmlBody, string TextBody) RenderReservationCreatedEmail(ReservationTemplateModel m)
        {
            var subject = $"{m.BrandName} — Nouvelle réservation {m.BookingId}";
            var innerText = new StringBuilder()
                .AppendLine($"Réservation {m.BookingId} pour le bateau {m.BoatName} (#{m.BoatId})")
                .AppendLine($"Locataire: {m.RenterName}")
                .AppendLine($"Période: {m.StartDate:yyyy-MM-dd} -> {m.EndDate:yyyy-MM-dd}")
                .AppendLine($"Total: {m.TotalPrice:C}")
                .ToString();

            var innerHtml = $"<p>Réservation <strong>{m.BookingId}</strong> pour le bateau <strong>{m.BoatName}</strong> (#{m.BoatId})</p>" +
                       $"<p>Locataire: {m.RenterName}</p>" +
                       $"<p>Période: {m.StartDate:yyyy-MM-dd} → {m.EndDate:yyyy-MM-dd}</p>" +
                       $"<p>Total: {m.TotalPrice:C}</p>";

            var html = BuildHtmlEnvelope(m.BrandName, subject, null, innerHtml);
            var text = BuildTextEnvelope(null, innerText);
            return (subject, html, text);
        }

        public (string Subject, string HtmlBody, string TextBody) RenderContactMessageEmail(ContactMessageTemplateModel m)
        {
            var subject = $"{m.BrandName} — Nouveau message de contact";

            // ---------- TEXT VERSION ----------
            var innerText =
                $"Nouveau message envoyé depuis la page de contact:\n\n" +
                $"Nom: {m.Name}\n" +
                $"Email: {m.Email}\n" +
                (string.IsNullOrWhiteSpace(m.Phone) ? "" : $"Téléphone: {m.Phone}\n") +
                $"Sujet: {m.Topic}\n\n" +
                $"Message:\n{m.Message}\n\n" +
                $"Envoyé le: {m.SentAt:u}";

            // ---------- HTML VERSION ----------
            var innerHtml =
                $"<p>Un nouveau message a été envoyé depuis la page <strong>Contact</strong> de {m.BrandName}.</p>" +
                $"<p><strong>Nom :</strong> {m.Name}</p>" +
                $"<p><strong>Email :</strong> {m.Email}</p>" +
                (string.IsNullOrWhiteSpace(m.Phone) ? "" : $"<p><strong>Téléphone :</strong> {m.Phone}</p>") +
                $"<p><strong>Sujet :</strong> {m.Topic}</p>" +
                $"<p style=\"margin-top:12px;\"><strong>Message :</strong><br>{m.Message}</p>" +
                $"<p style=\"margin-top:12px; color:#555;\">Envoyé le : {m.SentAt:u}</p>";

            // Wrap with envelope
            var html = BuildHtmlEnvelope(m.BrandName, subject, null, innerHtml);
            var text = BuildTextEnvelope(null, innerText);

            return (subject, html, text);
        }


        // Cancellation request: notify admin, owner and renter
        public (string Subject, string HtmlBody, string TextBody) RenderCancellationRequestEmail(CancellationRequestTemplateModel m)
        {
            var subject = $"{m.BrandName} — Demande d'annulation {m.BookingId}";
            var innerText = $"Demande d'annulation pour réservation {m.BookingId} (bateau {m.BoatName}):\nDemandeur: {m.RequesterName}\nMotif: {m.Reason}\nDemandé le: {m.RequestedAt:u}";
            var innerHtml = $"<p>Demande d'annulation pour la réservation <strong>{m.BookingId}</strong> (bateau <strong>{m.BoatName}</strong>)</p>" +
                       $"<p>Demandeur: {m.RequesterName}</p>" +
                       $"<p>Motif: {m.Reason}</p>" +
                       $"<p>Demandé le: {m.RequestedAt:u}</p>";

            var html = BuildHtmlEnvelope(m.BrandName, subject, null, innerHtml);
            var text = BuildTextEnvelope(null, innerText);
            return (subject, html, text);
        }

        // Document uploaded: notify admin and owner
        public (string Subject, string HtmlBody, string TextBody) RenderDocumentUploadedEmail(DocumentUploadedTemplateModel m)
        {
            var subject = $"{m.BrandName} — Document ajouté par {m.UserName}";
            var innerText = $"L'utilisateur {m.UserName} ({m.UserId}) a ajouté un document: {m.DocumentType}\nCommentaire: {m.Comment}\nUpload: {m.UploadedAt:u}";
            var innerHtml = $"<p>L'utilisateur <strong>{m.UserName}</strong> a ajouté un document: <strong>{m.DocumentType}</strong></p>" +
                       $"<p>Commentaire: {m.Comment}</p>" +
                       $"<p>Upload: {m.UploadedAt:u}</p>";

            var html = BuildHtmlEnvelope(m.BrandName, subject, null, innerHtml);
            var text = BuildTextEnvelope(null, innerText);
            return (subject, html, text);
        }

        // Reservation approved: notify renter and admin
        public (string Subject, string HtmlBody, string TextBody) RenderReservationApprovedEmail(ReservationApprovedTemplateModel m)
        {
            var subject = $"{m.BrandName} — Réservation confirmée {m.BookingId}";
            var innerText = $"Votre réservation {m.BookingId} pour {m.BoatName} du {m.StartDate:yyyy-MM-dd} au {m.EndDate:yyyy-MM-dd} a été approuvée.";
            var innerHtml = $"<p>Votre réservation <strong>{m.BookingId}</strong> pour <strong>{m.BoatName}</strong> du {m.StartDate:yyyy-MM-dd} au {m.EndDate:yyyy-MM-dd} a été approuvée.</p>";

            var html = BuildHtmlEnvelope(m.BrandName, subject, null, innerHtml);
            var text = BuildTextEnvelope(null, innerText);
            return (subject, html, text);
        }

        // Boat approved: notify owner
        public (string Subject, string HtmlBody, string TextBody) RenderBoatApprovedEmail(BoatApprovedTemplateModel m)
        {
            var subject = $"{m.BrandName} — Bateau approuvé #{m.BoatId}";
            var innerText = $"Votre bateau {m.BoatName} a été approuvé par l'administration.";
            var innerHtml = $"<p>Félicitations, votre bateau <strong>{m.BoatName}</strong> a été approuvé par l'administration.</p>";

            var html = BuildHtmlEnvelope(m.BrandName, subject, null, innerHtml);
            var text = BuildTextEnvelope(null, innerText);
            return (subject, html, text);
        }
    }
}
