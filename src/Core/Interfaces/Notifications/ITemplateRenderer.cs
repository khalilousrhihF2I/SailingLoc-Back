using Core.Models.Templates;
using Infrastructure.Models.Templates;

namespace Core.Interfaces.Services.Notifications
{
    public interface ITemplateRenderer
    {
        (string Subject, string HtmlBody, string TextBody) RenderResetCodeEmail(ResetCodeTemplateModel model);
        string RenderResetCodeSms(ResetCodeTemplateModel model);

        (string Subject, string HtmlBody, string TextBody) RenderNewUserCreatedEmail(NewUserTemplateModel model);

        (string Subject, string HtmlBody, string TextBody) RenderReservationCreatedEmail(ReservationTemplateModel model);

        (string Subject, string HtmlBody, string TextBody) RenderCancellationRequestEmail(CancellationRequestTemplateModel model);

        (string Subject, string HtmlBody, string TextBody) RenderDocumentUploadedEmail(DocumentUploadedTemplateModel model);

        (string Subject, string HtmlBody, string TextBody) RenderReservationApprovedEmail(ReservationApprovedTemplateModel model);

        (string Subject, string HtmlBody, string TextBody) RenderBoatApprovedEmail(BoatApprovedTemplateModel model);

        (string Subject, string HtmlBody, string TextBody) RenderContactMessageEmail(ContactMessageTemplateModel m);
    }
}
