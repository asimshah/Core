using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Fastnet.Core.Logging
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum LogFolderSetting

    {
        Normal, // uses environment ContentRootPath - good for web apps
        //Special, // uses Environment.SpecialFolder.MyDocuments
        Custom, // uses log folder in RollingFileLoggerOptions
    }
    /// <summary>
    /// 
    /// </summary>
    public class RollingFileLoggerOptions
    {
        // what should this type be used for?
        // In the microsoft logging stuff for the ConsoleLogger, the equivalent type
        //(ConsoleLoggerOptions) has a single property called IncludeScopes (which is also in 
        // ConsoleLoggerSettings!!

        // we need this type as it is used in the default constructor for the LoggerProvider
        // (there may be some other way of doing this - but there is no documentation that I can find)
        /// <summary>
        /// 
        /// </summary>
        public bool IncludeScopes { get; set; } = false;
        public LogFolderSetting LogFolderSetting { get; set; } = LogFolderSetting.Normal;
        public string LogFolder { get; set; }

    }
    internal class RollingFileLoggerSettings : IRollingFileLoggerSettings
    {
        public string AppName { get; set; }
        public IChangeToken ChangeToken { get; set; }
        public bool IncludeScopes { get; set; }
        public IDictionary<string, LogLevel> Switches { get; set; } = new Dictionary<string, LogLevel>();
        //public string LogFolder { get; set; }
        //public string AppFolder { get; set; }
        public IRollingFileLoggerSettings Reload()
        {
            return this;
        }
        public bool TryGetSwitch(string name, out LogLevel level)
        {
            return Switches.TryGetValue(name, out level);
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
