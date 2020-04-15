using System;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

