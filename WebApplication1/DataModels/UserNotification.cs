namespace UserAPI.DataModels
{
    public class UserNotification
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string NotificationId { get; set; }
        public Notification Notification { get; set; }
    }
}
