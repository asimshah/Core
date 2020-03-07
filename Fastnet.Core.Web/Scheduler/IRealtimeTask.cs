using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{
    [Obsolete("Withdrawn as does not work with aspnetcore 3.0 - use HostedService instead")]
    public interface IRealtimeTask
    {
        TimeSpan StartAfter { get; }
        //string Schedule { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
        Func<Task> BeforeTaskStartsAsync { get; }
        Func<Task> AfterTaskCompletesAsync { get; }
    }
}

