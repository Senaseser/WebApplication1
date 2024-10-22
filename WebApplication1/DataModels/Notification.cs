using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAPI.DataModels;

namespace UserAPI.DataModels
{
    public class Notification
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }
    }
}
