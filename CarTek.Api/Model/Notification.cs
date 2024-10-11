namespace CarTek.Api.Model
{
    public enum NotificationType
    {
        Information,
        Error
    }

    public class Notification
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public NotificationType NotificationType { get; set; }

        public DateTime DateSent { get; set; }

        public bool IsDriverNotification { get; set; }

        public long UserId { get; set; }
    }
}
