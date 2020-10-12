using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Fastnet.Core.Logging
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public interface IRollingFileLoggerSettings

    {
        //string AppName { get; }
        bool IncludeScopes { get; }
        IChangeToken ChangeToken { get; }
        bool TryGetSwitch(string name, out LogLevel level);
        IRollingFileLoggerSettings Reload();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
