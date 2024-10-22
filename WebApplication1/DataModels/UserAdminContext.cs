using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace UserAPI.DataModels
{
    public class UserAdminContext : IdentityDbContext
    {
        public UserAdminContext(DbContextOptions<UserAdminContext> options) : base(options)
        { }

        public DbSet<User> User { get; set; }

        public DbSet<Notification> Notification { get; set; }




    }
}
