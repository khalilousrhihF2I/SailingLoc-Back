using Core.Interfaces.Notifications;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Messaging
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly ISendGridClient _client;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string? _templateId;

        public SendGridEmailSender(ISendGridClient client, IConfiguration cfg)
        {
            _client = client;
            _fromEmail = cfg["SendGrid:FromEmail"] ?? "no-reply@yourapp.tld";
            _fromName = cfg["SendGrid:FromName"] ?? "SailingLoc";
            _templateId = cfg["SendGrid:TemplateId"];  
        }

        /// <summary>
        /// Envoi standard (texte/HTML simple)
        /// </summary>
        public async Task SendAsync(string toEmail, string subject, string htmlBody, string textBody, CancellationToken ct)
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_fromEmail, _fromName),
                Subject = subject,
                HtmlContent = htmlBody,
                PlainTextContent = textBody
            };
            msg.AddTo(new EmailAddress(toEmail));
            await _client.SendEmailAsync(msg, ct);
        }

        /// <summary>
        /// Envoi via un template dynamique SendGrid
        /// </summary>
        public async Task SendTemplateAsync(string toEmail, Dictionary<string, object> dynamicData, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_templateId))
                throw new InvalidOperationException("SendGrid:TemplateId is not configured.");

            var msg = new SendGridMessage
            {
                From = new EmailAddress(_fromEmail, _fromName),
                TemplateId = _templateId
            };
            msg.AddTo(new EmailAddress(toEmail));

            // Données dynamiques à injecter dans le template
            msg.SetTemplateData(dynamicData);

            await _client.SendEmailAsync(msg, ct);
        }
    }
}
