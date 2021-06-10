using Fastnet.Core.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Fastnet.Core.UserAccounts
{
    /// <summary>
    /// 
    /// </summary>
    public class UserAccount
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PasswordHash { get; set; }
        internal virtual ICollection<UserRole> UserRoles { get; } = new HashSet<UserRole>();
        /// <summary>
        /// /
        /// </summary>
        public IEnumerable<Role> Roles => UserRoles.Select(x => x.Role);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public UserAccountDTO ToDTO()
        {
            return new UserAccountDTO(Id, Email);
        }
    }
}
