using System.Collections.Generic;
using System.Linq;

namespace Fastnet.Core.UserAccounts
{
    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; }
        internal virtual ICollection<UserRole> UserRoles { get; } = new HashSet<UserRole>();
        public IEnumerable<UserAccount> Users => UserRoles.Select(x => x.User);
    }
}
