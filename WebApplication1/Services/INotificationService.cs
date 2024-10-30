using System.Threading.Tasks;
using UserAPI.DataModels;

namespace UserAPI.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotification(string userId, string title, string message);
    }
}
