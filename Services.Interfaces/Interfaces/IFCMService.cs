namespace Services.Interfaces.Interfaces
{
    public interface IFCMService
    {
        Task<bool> SendNotificationAsync(string deviceToken, string title, string body);
    }
}
