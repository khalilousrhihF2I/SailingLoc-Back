namespace Core.Interfaces.Services.Notifications
{
    public interface ISmsSender
    {
        Task SendAsync(string phoneNumber, string message, CancellationToken ct);
    }
}
