using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{
    /// <summary>
    /// Base class for tasks run by the SchedulerService in "real" time, i.e. continuously on startup.
    /// </summary>
    [Obsolete("Withdrawn as does now work with aspnetcore 3.0 - use HostedService instead")]
    public abstract class RealtimeTask : IRealtimeTask
    {
        private readonly ILoggerFactory loggerFactory;
        /// <summary>
        /// 
        /// </summary>
        protected ILogger log;
        /// <summary>
        /// Sets an delay interval before the task starts
        /// </summary>
        public TimeSpan StartAfter { get; }
        /// <summary>
        /// Function called beforee the Task starts
        /// </summary>
        public Func<Task> BeforeTaskStartsAsync { get; set; }
        /// <summary>
        /// Function called after the |task finishes
        /// </summary>
        public Func<Task> AfterTaskCompletesAsync { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerFactory"></param>
        public RealtimeTask(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            log = loggerFactory.CreateLogger(this.GetType().FullName);
        }
        /// <summary>
        /// Called to start the realtime task. On return, by default, the task
        /// enters a loop checking every 15 seconds for a cancellation. 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);
        /// <summary>
        /// override this to implement a custom loop to keep the task running
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(15000, cancellationToken);
            }
            log.Information($"CancellationRequested");
        }
        async Task IRealtimeTask.ExecuteAsync(CancellationToken cancellationToken)
        {
            log.Information($"Realtime task {this.GetType().Name} started");
            try
            {
                if (BeforeTaskStartsAsync != null)
                {
                    await BeforeTaskStartsAsync();
                }
            }
            catch (Exception xe)
            {
                log.Error(xe, $"BeforeTaskStartsAsync failed");
            }
            try
            {
                await ExecuteAsync(cancellationToken);
                await StartAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                log.Information($"{this.GetType().Name} execution cancelled");
            }
            catch (Exception xe)
            {
                this.log.Error(xe);
            }
            finally
            {
                try
                {
                    if (AfterTaskCompletesAsync != null)
                    {
                        await AfterTaskCompletesAsync();
                        //await AfterTaskCompletesAsync?.Invoke();
                    }
                }
                catch (Exception xe)
                {
                    log.Error(xe, $"AfterTaskCompletesAsync failed");
                }
            }
        }
    }
}

