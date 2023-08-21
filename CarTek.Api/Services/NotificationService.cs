using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Services.Interfaces;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly ApplicationDbContext _dbContext;

        public NotificationService(ILogger<NotificationService> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public Tuple<int, ICollection<Notification>> GetUserNotifications(bool isDriver, long userId, int pageNumber, int pageSize)
        {
            int totalNumber = 0;
            IQueryable<Notification> notifications;

            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            Expression<Func<Notification, object>> orderBy = x => x.DateSent;

            if (isDriver)
            {
                notifications = _dbContext.Notifications.Where(t => t.IsDriverNotification && t.UserId == userId).OrderByDescending(t => t.DateSent); 
                totalNumber = notifications.Count();
            }
            else
            {
                notifications = _dbContext.Notifications.Where(t => !t.IsDriverNotification && t.UserId == userId).OrderByDescending(t => t.DateSent); ;
                totalNumber = notifications.Count();
            }

            if (pageSize > 0)
            {
                notifications = notifications.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            return new Tuple<int, ICollection<Notification>>(totalNumber, notifications.ToList()); 
        }

        public async Task SendNotification(string title, string text, long userId, bool isDriver, string link = "")
        {
            var notification = new Notification
            {
                Title = title,
                DateSent = DateTime.UtcNow,
                UserId = userId,
                IsDriverNotification = isDriver,
                Description = text
            };

            _dbContext.Notifications.Add(notification);

            await _dbContext.SaveChangesAsync();
        }
    }
}
