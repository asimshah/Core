using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fastnet.Core.UserAccounts
{
    internal class UserRole
    {
        public long Id { get; set; }
        public UserAccount User { get; set; }
        public Role Role { get; set; }
    }
    public class UserAccountDb : DbContext
    {
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        internal DbSet<UserRole> UserRoles { get; set; }
        public UserAccountDb(DbContextOptions contextOptions) : base(contextOptions)
        {

        }
    }
}
