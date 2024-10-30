using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;


namespace UserAPI.DataModels
{
    public class ApplicationUser : IdentityUser
    {
     
        public bool IsOnline { get; set; }
        public ICollection<UserNotification> UserNotifications { get; set; }

    }
}
