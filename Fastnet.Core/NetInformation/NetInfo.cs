using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Fastnet.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class NetInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IPAddress> GetLocalIPV4Addresses()
        {
            var hostName = Dns.GetHostName();
            var entry = Dns.GetHostEntry(hostName);
            return entry.AddressList.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cidrRange"></param>
        /// <returns></returns>
        public static IEnumerable<IPAddress> GetMatchingIPV4Addresses(string cidrRange)
        {
            var addresses = GetLocalIPV4Addresses();
            return addresses.Where(a => a.ToIPNetwork().IsInIPRange(cidrRange));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cidrRange"></param>
        /// <returns></returns>
        public static IEnumerable<IPAddress> GetMatchingIPV4Addresses(IPNetwork cidrRange)
        {
            var addresses = GetLocalIPV4Addresses();
            return addresses.Where(a => a.ToIPNetwork().IsInIPRange(cidrRange));
        }
    }
}
