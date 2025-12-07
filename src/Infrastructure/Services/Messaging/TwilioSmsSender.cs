using Core.Interfaces.Services.Notifications;

namespace Infrastructure.Messaging
{
    // À implémenter si tu ajoutes Twilio (ou autre).
    public class TwilioSmsSender : ISmsSender
    {
        public Task SendAsync(string phoneNumber, string message, CancellationToken ct)
        {
            // TODO: intégrer Twilio/OVH/etc.
            return Task.CompletedTask;
        }
    }
}
