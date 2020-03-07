using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{
    ///// <summary>
    ///// should this be disposable???
    ///// </summary>
    //public abstract class HostedService :  IHostedService
    //{
    //    private CancellationTokenSource _cts;
    //    private Task _executingTask;
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="cancellationToken"></param>
    //    /// <returns></returns>
    //    private Task StartAsyncLocal(CancellationToken cancellationToken)
    //    {
    //        // Create a linked token so we can trigger cancellation outside of this token's cancellation
    //        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    //        // Store the task we're executing
    //        _executingTask = ExecuteAsync(_cts.Token);

    //        // If the task is completed then return it, otherwise it's running
    //        return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="cancellationToken"></param>
    //    /// <returns></returns>
    //    private async Task StopAsyncLocal(CancellationToken cancellationToken)
    //    {
    //        // Stop called without start
    //        if (_executingTask == null)
    //        {
    //            return;
    //        }

    //        // Signal cancellation to the executing method
    //        _cts.Cancel();

    //        // Wait until the task completes or the stop token triggers
    //        await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

    //        // Throw if cancellation triggered
    //        cancellationToken.ThrowIfCancellationRequested();
    //    }
    //    // Derived classes should override this and execute a long running method until 
    //    // cancellation is requested
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="cancellationToken"></param>
    //    /// <returns></returns>
    //    protected internal abstract Task ExecuteAsync(CancellationToken cancellationToken);

    //    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    //    {
    //        return StartAsyncLocal(cancellationToken);
    //    }

    //    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    //    {
    //        return StopAsyncLocal(cancellationToken);
    //    }
    //}

    /// <summary>
    /// Derive background services from this class and add them to IServicesCollection
    /// using AddService()
    /// </summary>
    public abstract class HostedService : BackgroundService
    {
        protected bool hasStarted = false;
        protected bool isStopping = false;
        protected bool hasStopped = false;
        protected readonly ILogger log;
        public HostedService(ILogger logger) : base()
        {
            this.log = logger;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var t = base.StartAsync(cancellationToken);
            hasStarted = true;
            return t;
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (!hasStopped)
            {
                OnStopped();
                var t = base.StopAsync(cancellationToken);
                hasStopped = true;
                hasStarted = false;
                return t;
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        protected virtual void OnStopped()
        {
            log.Debug("OnStopped has been called.");
        }
    }
}
