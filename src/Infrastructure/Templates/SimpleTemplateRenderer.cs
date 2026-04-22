using Core.Interfaces.Services.Notifications;
using Core.Models.Templates;
using Infrastructure.Models.Templates;
using System.Globalization;
using System.Text;

namespace Infrastructure.Templates
{
    /// <summary>
    /// Renders transactional emails for SailingLoc. All templates share a
    /// premium, email-client-safe layout: tables + inline styles, 600px max
    /// width, system font stack, ocean-blue brand palette, and a reusable set
    /// of building blocks (hero header, content card, info rows, CTA button,
    /// footer). Dates and currency are formatted in French.
    /// </summary>
    public class SimpleTemplateRenderer : ITemplateRenderer
    {
        // ── Brand palette (ocean theme, matches front-end Tailwind) ───────────
        private const string BrandPrimary   = "#0369a1"; // ocean-700
        private const string BrandPrimaryDk = "#075985"; // ocean-800 (gradient end)
        private const string BrandAccent    = "#0ea5e9"; // sky-500
        private const string TextPrimary    = "#0f172a"; // slate-900
        private const string TextMuted      = "#475569"; // slate-600
        private const string TextSubtle     = "#94a3b8"; // slate-400
        private const string Divider        = "#e2e8f0"; // slate-200
        private const string CardBg         = "#ffffff";
        private const string PageBg         = "#f1f5f9"; // slate-100

        private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

        // ─────────────────────────────────────────────────────────────────────
        // HTML building blocks
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Wraps the given inner HTML in a full, email-safe envelope. Produces a
        /// responsive 600px card with a branded gradient header, a title,
        /// optional preheader (inbox preview text) and a footer.
        /// </summary>
        private static string BuildHtmlEnvelope(
            string brandName,
            string subject,
            string? preheader,
            string heroTitle,
            string? heroSubtitle,
            string? recipientName,
            string innerHtml,
            string? supportEmail = null)
        {
            var greeting = string.IsNullOrWhiteSpace(recipientName)
                ? "Bonjour,"
                : $"Bonjour {HtmlEncode(recipientName)},";

            var preheaderHtml = string.IsNullOrWhiteSpace(preheader)
                ? string.Empty
                : $@"<div style=""display:none;max-height:0;overflow:hidden;opacity:0;visibility:hidden;mso-hide:all;font-size:1px;line-height:1px;color:{PageBg};"">{HtmlEncode(preheader)}</div>";

            var subtitleHtml = string.IsNullOrWhiteSpace(heroSubtitle)
                ? string.Empty
                : $@"<div style=""margin-top:6px;color:#e0f2fe;font-size:14px;line-height:20px;"">{HtmlEncode(heroSubtitle)}</div>";

            var footerSupport = string.IsNullOrWhiteSpace(supportEmail)
                ? string.Empty
                : $@"<br/>Besoin d'aide ? <a href=""mailto:{HtmlAttr(supportEmail)}"" style=""color:{BrandPrimary};text-decoration:none;"">{HtmlEncode(supportEmail)}</a>";

            return $@"<!doctype html>
<html lang=""fr"">
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
    <meta name=""x-apple-disable-message-reformatting"">
    <meta name=""color-scheme"" content=""light"">
    <meta name=""supported-color-schemes"" content=""light"">
    <title>{HtmlEncode(subject)}</title>
  </head>
  <body style=""margin:0;padding:0;background:{PageBg};font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:{TextPrimary};"">
    {preheaderHtml}
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:{PageBg};"">
      <tr>
        <td align=""center"" style=""padding:32px 16px;"">
          <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:600px;width:100%;background:{CardBg};border-radius:16px;overflow:hidden;box-shadow:0 4px 16px rgba(15,23,42,0.06);"">
            <!-- Hero -->
            <tr>
              <td style=""background:linear-gradient(135deg,{BrandPrimary} 0%,{BrandPrimaryDk} 100%);background-color:{BrandPrimary};padding:28px 32px;color:#ffffff;"">
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                  <tr>
                    <td style=""font-size:13px;letter-spacing:2px;text-transform:uppercase;color:#bae6fd;font-weight:600;"">⚓ {HtmlEncode(brandName)}</td>
                  </tr>
                  <tr>
                    <td style=""padding-top:10px;font-size:22px;font-weight:700;line-height:28px;color:#ffffff;"">{HtmlEncode(heroTitle)}</td>
                  </tr>
                  <tr><td>{subtitleHtml}</td></tr>
                </table>
              </td>
            </tr>
            <!-- Body -->
            <tr>
              <td style=""padding:32px;"">
                <p style=""margin:0 0 16px 0;font-size:16px;line-height:24px;color:{TextPrimary};"">{greeting}</p>
                <div style=""font-size:15px;line-height:22px;color:{TextMuted};"">
                  {innerHtml}
                </div>
              </td>
            </tr>
            <!-- Footer -->
            <tr>
              <td style=""padding:20px 32px;border-top:1px solid {Divider};background:#fafafa;"">
                <div style=""font-size:12px;line-height:18px;color:{TextSubtle};text-align:center;"">
                  © {DateTime.UtcNow.Year} {HtmlEncode(brandName)} — Tous droits réservés.{footerSupport}
                </div>
              </td>
            </tr>
          </table>
          <div style=""font-size:11px;color:{TextSubtle};padding:12px;max-width:600px;line-height:16px;"">
            Vous recevez cet email parce que vous êtes inscrit(e) sur {HtmlEncode(brandName)}.
          </div>
        </td>
      </tr>
    </table>
  </body>
</html>";
        }

        /// <summary>Two-column "label : value" row for key info blocks.</summary>
        private static string BuildInfoRow(string label, string value, bool isLast = false)
        {
            var borderBottom = isLast ? "" : $"border-bottom:1px solid {Divider};";
            return $@"<tr>
  <td style=""padding:10px 0;{borderBottom}font-size:14px;color:{TextSubtle};width:40%;vertical-align:top;"">{HtmlEncode(label)}</td>
  <td style=""padding:10px 0;{borderBottom}font-size:14px;color:{TextPrimary};font-weight:600;vertical-align:top;"">{value}</td>
</tr>";
        }

        /// <summary>Bordered info card wrapping a table of info rows.</summary>
        private static string BuildInfoCard(string innerRows)
            => $@"<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin:20px 0;background:#f8fafc;border:1px solid {Divider};border-radius:12px;padding:8px 20px;"">{innerRows}</table>";

        /// <summary>Small colored status pill.</summary>
        private static string BuildStatusPill(string text, string color, string bg)
            => $@"<span style=""display:inline-block;padding:4px 12px;font-size:12px;font-weight:600;color:{color};background:{bg};border-radius:999px;letter-spacing:0.3px;"">{HtmlEncode(text)}</span>";

        /// <summary>Paragraph styled for email body copy.</summary>
        private static string P(string html)
            => $@"<p style=""margin:0 0 14px 0;font-size:15px;line-height:22px;color:{TextMuted};"">{html}</p>";

        // ─────────────────────────────────────────────────────────────────────
        // Plain-text helpers
        // ─────────────────────────────────────────────────────────────────────

        private static string BuildTextEnvelope(string? recipientName, string innerText, string? supportEmail = null, string brandName = "SailingLoc")
        {
            var sb = new StringBuilder();
            sb.AppendLine($"── {brandName} ──");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(recipientName)) sb.AppendLine($"Bonjour {recipientName},");
            sb.AppendLine();
            sb.AppendLine(innerText.TrimEnd());
            sb.AppendLine();
            sb.AppendLine("——————————————————————————");
            if (!string.IsNullOrWhiteSpace(supportEmail))
                sb.AppendLine($"Besoin d'aide ? {supportEmail}");
            sb.AppendLine($"© {DateTime.UtcNow.Year} {brandName}");
            return sb.ToString();
        }

        private static string HtmlEncode(string? s)
            => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);

        private static string HtmlAttr(string? s)
            => (s ?? string.Empty).Replace("\"", "&quot;");

        private static string FormatDate(DateTime d) => d.ToString("dddd d MMMM yyyy", Fr);
        private static string FormatDateShort(DateTime d) => d.ToString("d MMMM yyyy", Fr);
        private static string FormatDateTime(DateTime d) => d.ToString("d MMMM yyyy 'à' HH:mm", Fr);
        private static string FormatMoney(decimal v) => v.ToString("C0", Fr);

        // ─────────────────────────────────────────────────────────────────────
        // Templates
        // ─────────────────────────────────────────────────────────────────────

        public (string Subject, string HtmlBody, string TextBody) RenderResetCodeEmail(ResetCodeTemplateModel m)
        {
            var subject = $"{m.BrandName} — Code de réinitialisation";
            var preheader = $"Votre code : {m.Code} (expire dans {m.ExpiresMinutes} min)";

            var innerHtml = new StringBuilder()
                .Append(P("Vous avez demandé la réinitialisation de votre mot de passe. Utilisez le code ci-dessous dans l'application pour continuer :"))
                .Append($@"<div style=""margin:28px 0;text-align:center;"">
  <div style=""display:inline-block;padding:20px 32px;background:#f0f9ff;border:2px dashed {BrandAccent};border-radius:14px;"">
    <div style=""font-family:'SF Mono','Consolas',monospace;font-size:32px;font-weight:700;letter-spacing:10px;color:{BrandPrimary};"">{HtmlEncode(m.Code)}</div>
  </div>
</div>")
                .Append(P($"Ce code expire dans <strong style=\"color:{TextPrimary};\">{m.ExpiresMinutes} minutes</strong>."))
                .Append(P($"<span style=\"color:{TextSubtle};font-size:13px;\">Si vous n'êtes pas à l'origine de cette demande, ignorez simplement cet email — votre mot de passe restera inchangé.</span>"))
                .ToString();

            var textBody = BuildTextEnvelope(m.RecipientName,
                $"Votre code de réinitialisation : {m.Code}\n" +
                $"Il expire dans {m.ExpiresMinutes} minutes.\n\n" +
                $"Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.",
                m.SupportEmail, m.BrandName);

            var html = BuildHtmlEnvelope(
                m.BrandName, subject, preheader,
                heroTitle: "Réinitialisation du mot de passe",
                heroSubtitle: "Un code à usage unique vous est envoyé",
                recipientName: m.RecipientName,
                innerHtml: innerHtml,
                supportEmail: m.SupportEmail);

            return (subject, html, textBody);
        }

        public string RenderResetCodeSms(ResetCodeTemplateModel m)
            => $"{m.BrandName}: votre code est {m.Code}. Expire dans {m.ExpiresMinutes} min.";

        public (string Subject, string HtmlBody, string TextBody) RenderNewUserCreatedEmail(NewUserTemplateModel m)
        {
            var subject = $"{m.BrandName} — Nouvel utilisateur enregistré";
            var preheader = $"Nouvel utilisateur : {m.UserName}";

            var rows = new StringBuilder()
                .Append(BuildInfoRow("Nom", HtmlEncode(m.UserName)))
                .Append(BuildInfoRow("Email", $"<a href=\"mailto:{HtmlAttr(m.UserEmail)}\" style=\"color:{BrandPrimary};text-decoration:none;\">{HtmlEncode(m.UserEmail)}</a>"))
                .Append(BuildInfoRow("Date", HtmlEncode(FormatDateTime(m.CreatedAt)), isLast: true))
                .ToString();

            var innerHtml = P("Un nouveau compte vient d'être créé sur la plateforme.")
                          + BuildInfoCard(rows)
                          + P($"<span style=\"color:{TextSubtle};font-size:13px;\">Cet email est envoyé automatiquement à l'équipe administrative.</span>");

            var text = BuildTextEnvelope(null,
                $"Nouvel utilisateur enregistré :\n- Nom : {m.UserName}\n- Email : {m.UserEmail}\n- Date : {FormatDateTime(m.CreatedAt)}",
                brandName: m.BrandName);

            var html = BuildHtmlEnvelope(m.BrandName, subject, preheader,
                heroTitle: "Nouvel utilisateur", heroSubtitle: "Un compte vient d'être créé",
                recipientName: null, innerHtml: innerHtml);

            return (subject, html, text);
        }

        public (string Subject, string HtmlBody, string TextBody) RenderReservationCreatedEmail(ReservationTemplateModel m)
        {
            var subject = $"{m.BrandName} — Nouvelle réservation · {m.BoatName}";
            var preheader = $"Réservation {m.BookingId} · {FormatDateShort(m.StartDate)} → {FormatDateShort(m.EndDate)}";

            var rows = new StringBuilder()
                .Append(BuildInfoRow("Référence", $"<span style=\"font-family:monospace;\">{HtmlEncode(m.BookingId)}</span>"))
                .Append(BuildInfoRow("Bateau", HtmlEncode(m.BoatName)))
                .Append(BuildInfoRow("Locataire", HtmlEncode(m.RenterName)))
                .Append(BuildInfoRow("Du", HtmlEncode(FormatDate(m.StartDate))))
                .Append(BuildInfoRow("Au", HtmlEncode(FormatDate(m.EndDate))))
                .Append(BuildInfoRow("Montant total", $"<span style=\"color:{BrandPrimary};font-size:16px;\">{HtmlEncode(FormatMoney(m.TotalPrice))}</span>", isLast: true))
                .ToString();

            var innerHtml = P("Une nouvelle réservation vient d'être créée. Voici les détails :")
                          + BuildInfoCard(rows)
                          + $"<div style=\"text-align:center;margin-top:8px;\">{BuildStatusPill("À confirmer", "#b45309", "#fef3c7")}</div>";

            var text = BuildTextEnvelope(null,
                $"Nouvelle réservation {m.BookingId}\n" +
                $"Bateau : {m.BoatName} (#{m.BoatId})\n" +
                $"Locataire : {m.RenterName}\n" +
                $"Période : {FormatDate(m.StartDate)} → {FormatDate(m.EndDate)}\n" +
                $"Total : {FormatMoney(m.TotalPrice)}",
                brandName: m.BrandName);

            var html = BuildHtmlEnvelope(m.BrandName, subject, preheader,
                heroTitle: "Nouvelle réservation", heroSubtitle: $"Réservation {m.BookingId}",
                recipientName: null, innerHtml: innerHtml);

            return (subject, html, text);
        }

        public (string Subject, string HtmlBody, string TextBody) RenderContactMessageEmail(ContactMessageTemplateModel m)
        {
            var subject = $"{m.BrandName} — Message de contact · {m.Topic}";
            var preheader = $"De {m.Name} · {m.Topic}";

            var rowsBuilder = new StringBuilder()
                .Append(BuildInfoRow("Nom", HtmlEncode(m.Name)))
                .Append(BuildInfoRow("Email", $"<a href=\"mailto:{HtmlAttr(m.Email)}\" style=\"color:{BrandPrimary};text-decoration:none;\">{HtmlEncode(m.Email)}</a>"));
            if (!string.IsNullOrWhiteSpace(m.Phone))
                rowsBuilder.Append(BuildInfoRow("Téléphone", HtmlEncode(m.Phone!)));
            rowsBuilder
                .Append(BuildInfoRow("Sujet", HtmlEncode(m.Topic)))
                .Append(BuildInfoRow("Envoyé le", HtmlEncode(FormatDateTime(m.SentAt)), isLast: true));

            var messageBlock = $@"<div style=""margin:20px 0;padding:20px;background:#f8fafc;border-left:4px solid {BrandAccent};border-radius:8px;"">
  <div style=""font-size:12px;text-transform:uppercase;letter-spacing:1px;color:{TextSubtle};margin-bottom:10px;font-weight:600;"">Message</div>
  <div style=""white-space:pre-wrap;font-size:15px;line-height:22px;color:{TextPrimary};"">{HtmlEncode(m.Message)}</div>
</div>";

            var innerHtml = P("Un nouveau message a été envoyé depuis la page de contact.")
                          + BuildInfoCard(rowsBuilder.ToString())
                          + messageBlock;

            var text = BuildTextEnvelope(null,
                $"Nouveau message de contact\n" +
                $"- Nom : {m.Name}\n" +
                $"- Email : {m.Email}\n" +
                (string.IsNullOrWhiteSpace(m.Phone) ? "" : $"- Téléphone : {m.Phone}\n") +
                $"- Sujet : {m.Topic}\n" +
                $"- Envoyé le : {FormatDateTime(m.SentAt)}\n\n" +
                $"Message :\n{m.Message}",
                brandName: m.BrandName);

            var html = BuildHtmlEnvelope(m.BrandName, subject, preheader,
                heroTitle: "Message de contact", heroSubtitle: m.Topic,
                recipientName: null, innerHtml: innerHtml);

            return (subject, html, text);
        }

        public (string Subject, string HtmlBody, string TextBody) RenderCancellationRequestEmail(CancellationRequestTemplateModel m)
        {
            var subject = $"{m.BrandName} — Demande d'annulation · {m.BookingId}";
            var preheader = $"Annulation demandée pour {m.BoatName}";

            var rows = new StringBuilder()
                .Append(BuildInfoRow("Réservation", $"<span style=\"font-family:monospace;\">{HtmlEncode(m.BookingId)}</span>"))
                .Append(BuildInfoRow("Bateau", HtmlEncode(m.BoatName)))
                .Append(BuildInfoRow("Demandeur", HtmlEncode(m.RequesterName)))
                .Append(BuildInfoRow("Demandé le", HtmlEncode(FormatDateTime(m.RequestedAt)), isLast: true))
                .ToString();

            var reasonBlock = $@"<div style=""margin:20px 0;padding:18px;background:#fef2f2;border-left:4px solid #ef4444;border-radius:8px;"">
  <div style=""font-size:12px;text-transform:uppercase;letter-spacing:1px;color:#991b1b;margin-bottom:8px;font-weight:600;"">Motif invoqué</div>
  <div style=""white-space:pre-wrap;font-size:14px;line-height:20px;color:{TextPrimary};"">{HtmlEncode(m.Reason)}</div>
</div>";

            var innerHtml = P("Une demande d'annulation vient d'être soumise.")
                          + BuildInfoCard(rows)
                          + reasonBlock
                          + $"<div style=\"text-align:center;margin-top:8px;\">{BuildStatusPill("En attente de traitement", "#991b1b", "#fee2e2")}</div>";

            var text = BuildTextEnvelope(null,
                $"Demande d'annulation\n" +
                $"- Réservation : {m.BookingId}\n" +
                $"- Bateau : {m.BoatName}\n" +
                $"- Demandeur : {m.RequesterName}\n" +
                $"- Motif : {m.Reason}\n" +
                $"- Demandé le : {FormatDateTime(m.RequestedAt)}",
                brandName: m.BrandName);

            var html = BuildHtmlEnvelope(m.BrandName, subject, preheader,
                heroTitle: "Demande d'annulation", heroSubtitle: $"Réservation {m.BookingId}",
                recipientName: null, innerHtml: innerHtml);

            return (subject, html, text);
        }

        public (string Subject, string HtmlBody, string TextBody) RenderDocumentUploadedEmail(DocumentUploadedTemplateModel m)
        {
            var subject = $"{m.BrandName} — Nouveau document · {m.DocumentType}";
            var preheader = $"{m.UserName} a ajouté : {m.DocumentType}";

            var rowsBuilder = new StringBuilder()
                .Append(BuildInfoRow("Utilisateur", HtmlEncode(m.UserName)))
                .Append(BuildInfoRow("Type de document", HtmlEncode(m.DocumentType)))
                .Append(BuildInfoRow("Ajouté le", HtmlEncode(FormatDateTime(m.UploadedAt)),
                    isLast: string.IsNullOrWhiteSpace(m.Comment)));
            if (!string.IsNullOrWhiteSpace(m.Comment))
                rowsBuilder.Append(BuildInfoRow("Commentaire", HtmlEncode(m.Comment!), isLast: true));

            var innerHtml = P("Un nouveau document a été déposé et attend une vérification.")
                          + BuildInfoCard(rowsBuilder.ToString());

            var text = BuildTextEnvelope(null,
                $"Nouveau document déposé\n" +
                $"- Utilisateur : {m.UserName}\n" +
                $"- Type : {m.DocumentType}\n" +
                (string.IsNullOrWhiteSpace(m.Comment) ? "" : $"- Commentaire : {m.Comment}\n") +
                $"- Ajouté le : {FormatDateTime(m.UploadedAt)}",
                brandName: m.BrandName);

            var html = BuildHtmlEnvelope(m.BrandName, subject, preheader,
                heroTitle: "Nouveau document", heroSubtitle: m.DocumentType,
                recipientName: null, innerHtml: innerHtml);

            return (subject, html, text);
        }

        public (string Subject, string HtmlBody, string TextBody) RenderReservationApprovedEmail(ReservationApprovedTemplateModel m)
        {
            var subject = $"{m.BrandName} — Réservation confirmée · {m.BoatName}";
            var preheader = $"Votre réservation {m.BookingId} est confirmée";

            var rows = new StringBuilder()
                .Append(BuildInfoRow("Référence", $"<span style=\"font-family:monospace;\">{HtmlEncode(m.BookingId)}</span>"))
                .Append(BuildInfoRow("Bateau", HtmlEncode(m.BoatName)))
                .Append(BuildInfoRow("Départ", HtmlEncode(FormatDate(m.StartDate))))
                .Append(BuildInfoRow("Retour", HtmlEncode(FormatDate(m.EndDate)), isLast: true))
                .ToString();

            var innerHtml = P($"Excellente nouvelle ! Votre réservation pour <strong style=\"color:{TextPrimary};\">{HtmlEncode(m.BoatName)}</strong> vient d'être confirmée.")
                          + $"<div style=\"text-align:center;margin:8px 0 24px 0;\">{BuildStatusPill("Confirmée", "#065f46", "#d1fae5")}</div>"
                          + BuildInfoCard(rows)
                          + P("Vous pouvez retrouver tous les détails de votre location dans votre espace personnel. Bonne navigation !");

            var text = BuildTextEnvelope(m.RenterName,
                $"Votre réservation {m.BookingId} est confirmée.\n" +
                $"- Bateau : {m.BoatName}\n" +
                $"- Départ : {FormatDate(m.StartDate)}\n" +
                $"- Retour : {FormatDate(m.EndDate)}\n\n" +
                $"Bonne navigation !",
                brandName: m.BrandName);

            var html = BuildHtmlEnvelope(m.BrandName, subject, preheader,
                heroTitle: "Réservation confirmée", heroSubtitle: "Préparez vos affaires — tout est prêt !",
                recipientName: m.RenterName, innerHtml: innerHtml);

            return (subject, html, text);
        }

        public (string Subject, string HtmlBody, string TextBody) RenderBoatApprovedEmail(BoatApprovedTemplateModel m)
        {
            var subject = $"{m.BrandName} — Votre bateau est en ligne !";
            var preheader = $"{m.BoatName} est désormais visible par les locataires";

            var innerHtml = P($"Félicitations <strong style=\"color:{TextPrimary};\">{HtmlEncode(m.OwnerName)}</strong> ! Votre annonce vient d'être approuvée par notre équipe.")
                          + $"<div style=\"text-align:center;margin:24px 0;\">{BuildStatusPill("Publiée", "#065f46", "#d1fae5")}</div>"
                          + BuildInfoCard(
                                BuildInfoRow("Bateau", HtmlEncode(m.BoatName)) +
                                BuildInfoRow("Identifiant", $"#{m.BoatId}", isLast: true))
                          + P("Votre bateau est dès à présent visible par les locataires. Nous vous souhaitons de belles locations sur SailingLoc.");

            var text = BuildTextEnvelope(m.OwnerName,
                $"Votre bateau « {m.BoatName} » (#{m.BoatId}) a été approuvé et publié.\n" +
                $"Il est désormais visible par les locataires.",
                brandName: m.BrandName);

            var html = BuildHtmlEnvelope(m.BrandName, subject, preheader,
                heroTitle: "Annonce approuvée", heroSubtitle: "Votre bateau est en ligne",
                recipientName: m.OwnerName, innerHtml: innerHtml);

            return (subject, html, text);
        }
    }
}
