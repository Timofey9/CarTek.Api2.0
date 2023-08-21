using CarTek.Api.Model;

namespace CarTek.Api.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotification(string title, string text, long userId, bool isDriver, string link = "");

        Tuple<int,ICollection<Notification>> GetUserNotifications(bool isDriver, long userId, int pageNumber, int pageSize);
    }
}
