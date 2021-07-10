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
    /// <summary>
    /// 
    /// </summary>
    public class UserAccountDb : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        public DbSet<UserAccount> UserAccounts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DbSet<Role> Roles { get; set; }
        internal DbSet<UserRole> UserRoles { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextOptions"></param>
        public UserAccountDb(DbContextOptions<UserAccountDb> contextOptions) : base(contextOptions)
        {

        }
    }
}
