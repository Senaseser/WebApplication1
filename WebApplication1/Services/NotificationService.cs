using System.Threading;
using System;
using System.Threading.Tasks;
using UserAPI.DataModels;

namespace UserAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly UserAdminContext _context;
        public NotificationService(UserAdminContext context)
        {
            _context = context;
        }
        public async Task<Notification> CreateNotification(string userId, string title, string message)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Message = message,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _context.Notification.AddAsync(notification);
            await _context.SaveChangesAsync();

            return notification;
        }
    }
}

