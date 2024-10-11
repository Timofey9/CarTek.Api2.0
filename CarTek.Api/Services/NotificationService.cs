using Amazon.Auth.AccessControlPolicy;
using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Services.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Notification = FirebaseAdmin.Messaging.Notification;

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

            InitializeFirebase();
        }

        public Tuple<int, ICollection<Model.Notification>> GetUserNotifications(bool isDriver, long userId, int pageNumber, int pageSize)
        {
            int totalNumber = 0;
            IQueryable<Model.Notification> notifications;

            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            Expression<Func<Model.Notification, object>> orderBy = x => x.DateSent;

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

            return new Tuple<int, ICollection<Model.Notification>>(totalNumber, notifications.ToList()); 
        }

        public async Task SaveToken(long userId, string token, bool isDriver)
        {
            var newUserDevice = new UserDevice
            {
                UserId = userId,
                Token = token,
                IsDriver = isDriver
            };

            _dbContext.UserDevices.Add(newUserDevice);

            await _dbContext.SaveChangesAsync();

        }

        private void InitializeFirebase()
        {
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.GetApplicationDefault(),
                    });
                }
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex.Message, ex);
            }
        }

        public async Task SendNotification(string title, string text, long userId, bool isDriver, string link = "")
        {
            try
            {
                var tokens = _dbContext.UserDevices.Where(ud => ud.IsDriver == isDriver && ud.UserId == userId).Select(t => t.Token).ToList();

                if(tokens != null && tokens.Count > 0) { 
                    var message = new MulticastMessage()
                    {
                        Tokens = tokens,
                        Notification = new Notification()
                        {
                            Title = title,
                            Body = text,
                        }
                    };

                    var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                }

                var notification = new Model.Notification
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
            catch(Exception ex)
            {
                _logger.LogError($"Ошибка отправки уведомления пользователю:{userId}, {ex.Message}, stack: {ex.StackTrace}, ie: {ex.InnerException?.Message}", ex.Message);
            }
        }
    }
}
