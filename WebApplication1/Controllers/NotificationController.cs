using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserAPI.DataModels;
using UserAPI.Services;

namespace UserAPI.Controllers
{

    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly INotificationService _notificationService;
  
        public NotificationController(IAuthService authService, UserManager<ApplicationUser> userManager,IHubContext<NotificationHub> hubContext,IRabbitMqService rabbitMqService,INotificationService notificationService)
        {
            _userManager = userManager;
            _authService = authService;
            _hubContext = hubContext;
            _rabbitMqService = rabbitMqService;
            _notificationService = notificationService;

        }
        [HttpPost("Send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDto notificationDto)
        {
            var userIds = notificationDto.UserIds;
            if (userIds == null || !userIds.Any())
            {
                return BadRequest("Kullanıcı ID listesi boş olamaz.");
            }
            var notFoundUsers = new List<string>();
            var sentNotifications = new List<string>();

            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if(user == null)
                {
                    notFoundUsers.Add(userId);
                    continue;
                }
                var notification = await _notificationService.CreateNotification(user.Id, notificationDto.Title, notificationDto.Message);
                if (user.IsOnline)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                    {
                        UserId = user.Id,
                        Title = notificationDto.Title,
                        Message = notificationDto.Message
                    });
                }
                else
                {
                    await _rabbitMqService.PublishNotificationToQueue(new RabbitMqData { 
                        UserId = user.Id,
                        Title= notificationDto.Title, 
                        Message=notificationDto.Message});
                    sentNotifications.Add(userId + " (RabbitMQ'ya eklendi)"); 
                }
            }
       
                return Ok(new
                {
                    Message = "Bildirim gönderme işlemi tamamlandı",
                    SentNotification = sentNotifications,
                    NotFoundUsers = notFoundUsers.Any() ? notFoundUsers : null
                });


        }
    }
}
