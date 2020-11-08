using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;

namespace Fastnet.Core
{
    public static partial class Extensions
    {
        internal static void ConfigureMessengerOptions(this IServiceCollection services)
        {
            services.ConfigureOptions<ConfigureMessengerOptions>();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    internal class ConfigureMessengerOptions : IConfigureOptions<MessengerOptions>
    {
        private readonly IHostEnvironment environment;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        public ConfigureMessengerOptions(IHostEnvironment env)
        {
            this.environment = env;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public void Configure(MessengerOptions options)
        {
            if (options.AutoConfigureMulticastIPAddress)
            {
                var localMachine = Environment.MachineName.ToLower();
                //
                if (this.environment.IsDevelopment())
                {
                    switch (localMachine)
                    {
                        case "small-box":
                        case "music-box":
                        case "black-box":
                            options.MulticastIPAddress = "224.100.0.64";
                            break;
                        case "asus": //my laptop
                            options.MulticastIPAddress = "224.100.0.65";
                            break;
                    }
                }
                else
                {
                    switch (localMachine)
                    {
                        case "small-box":
                        case "music-box":
                        case "black-box":
                            options.MulticastIPAddress = "224.100.0.2";
                            break;
                        case "asus": //my laptop
                            options.MulticastIPAddress = "224.100.0.3";
                            break;
                    }
                } 
            }
        }
    }
    /// <summary>
    /// options for the messaging system
    /// </summary>
    public class MessengerOptions //: Options
    {
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
        public string LocalCIDR { get; set; }
        /// <summary>
        /// If true, the MulticastIPAddress is set according to the current machine name and whether running in development
        /// NB: requires services.ConfigureCoreServices() to be called in Startup.ConfigureServices() (aspnetcore)
        /// </summary>
        public bool AutoConfigureMulticastIPAddress { get; set; } = true;
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
