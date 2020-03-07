using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Fastnet.Core.Logging
{
    internal interface IRollingFileLoggerSettings
    {
        //string AppName { get; }
        bool IncludeScopes { get; }
        IChangeToken ChangeToken { get; }
        bool TryGetSwitch(string name, out LogLevel level);
        IRollingFileLoggerSettings Reload();
    }
}
