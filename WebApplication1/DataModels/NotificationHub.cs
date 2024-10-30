using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using UserAPI.Services;

namespace UserAPI.DataModels
{
    public class NotificationHub : Hub
    {
        private readonly NotificationService _notificationService;
        public NotificationHub(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        public async Task SetUserOnline(string userId)
        {
            await Clients.All.SendAsync("UserOnline", userId);
        }
        public async Task SetUserOffline(string userId)
        {
            await Clients.All.SendAsync("UserOffline", userId);

        }
        public async Task SendNotification(string userId, string title, string message)
        {
            await _notificationService.CreateNotification(userId, title, message);

        }
    }
}
