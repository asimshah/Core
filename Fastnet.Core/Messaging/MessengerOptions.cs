namespace Fastnet.Core
{
    /// <summary>
    /// options for the messaging system
    /// </summary>
    public class MessengerOptions //: Options
    {
        //[Obsolete]
        //public bool TraceMessages { get; set; }
        //public bool TraceSerialization { get; set; }
        /// <summary>
        /// Maximum message length, default = 4096 * 64, should not need to be altered
        /// </summary>
        public int MaxMessageSize { get; set; }
        /// <summary>
        /// Internal message buffer, default = 4096 * 8, do not change (normally)
        /// </summary>
        public int TransportBufferSize { get; set; }
        /// <summary>
        /// Ensures that multicast listening is not started automatically
        /// </summary>
        public bool DisableMulticastListening { get; set; }
        /// <summary>
        /// default address is 224.100.0.1
        /// </summary>
        public string MulticastIPAddress { get; set; }
        /// <summary>
        /// default port 9050
        /// </summary>
        public int MulticastPort { get; set; }
        /// <summary>
        /// default address is 192.168.0.0/24
        /// </summary>
        public string LocalCIDR { get; set; } // should be called LocalCIDR
        /// <summary>
        /// 
        /// </summary>
        public MessengerOptions()
        {
            MaxMessageSize = 4096 * 64;
            TransportBufferSize = 4096 * 8;
            MulticastIPAddress = "224.100.0.1";
            MulticastPort = 9050;
            LocalCIDR = "192.168.0.0/24";
        }
    }
}
