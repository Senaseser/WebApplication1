using System.Collections.Generic;
using System.Web;

namespace UserAPI.DataModels
{
    public class NotificationDto
    {
        public List<string> UserIds { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }
    }
}
