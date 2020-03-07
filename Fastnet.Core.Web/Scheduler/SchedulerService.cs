using Fastnet.Core.Cron;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{
    /// <summary>
    /// Runs IScheduledTasks in the background
    /// Add each IScheduledTask as a singleton in the ConfigureServices() method of Startup
    /// This is an IHostedService and is started automatically by aspnetcore
    /// </summary>
    public class SchedulerService : HostedService
    {
        private class ScheduledTaskWrapper
        {
            public CrontabSchedule Schedule { get; set; }
            public bool ManualStartOnly { get; set; }
            public IScheduledTask Task { get; set; }
            public DateTime LastRunTime { get; set; }
            public DateTime NextRunTime { get; set; }
            public void Increment()
            {
                LastRunTime = NextRunTime;
                NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
            }
            public bool IsRunning { get; set; }
            public bool ShouldRun(DateTime currentTime)
            {
                return NextRunTime < currentTime && LastRunTime != NextRunTime;
            }
            public string ToReportingLine()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(this.Task.GetType().Name);
                if (this.Task is ScheduledTask && (this.Task as ScheduledTask).Pipeline.Count > 1)
                {
                    sb.Append($" (pipeline of {(this.Task as ScheduledTask).Pipeline.Count} items)");
                }
                sb.Append($", schedule: {CrontabSchedule.GetDescription((Task as ScheduledTask).Schedule)}");
                if (IsRunning)
                {
                    sb.Append(", ** is running **");
                }
                else
                {
                    sb.Append($", will next run at {NextRunTime.ToLocalTime().ToDefaultWithTime()}");
                }
                return sb.ToString();
            }
        }
        private readonly IServiceProvider sp;
        private readonly SchedulerOptions options;
        //private readonly ILogger log;
        /// <summary>
        /// handler to call if a sheduled task throws an exception
        /// (if no handler is provided, task will re-throw the exception)
        /// </summary>
        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;
        private readonly List<ScheduledTaskWrapper> _scheduledTasks = new List<ScheduledTaskWrapper>();
        //internal IEnumerable<IRealtimeTask> realtimeTasks;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="logger"></param>
        /// <param name="_options"></param>
        public SchedulerService(IServiceProvider sp,  ILogger<SchedulerService> logger, IOptions<SchedulerOptions> _options) : base(logger)
        {
            //this.log = logger;
            this.options = _options.Value;
            this.sp = sp;
            if (Debugger.IsAttached && options.DebugBreak)
            {
                Debugger.Break();

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TimeSpan pollInterval = TimeSpan.FromSeconds(30); // interval between checking for tasks that should be run
            Initialise();
            //StartRealtimeTasks();
            while (!options.SuspendScheduling && !cancellationToken.IsCancellationRequested)
            {
                if (options.TraceTaskPolling)
                {
                    log.Trace($"ExecuteAsync(): polling {_scheduledTasks.Count()} tasks");
                }
                await ExecuteScheduledTasks(cancellationToken);

                await Task.Delay(pollInterval, cancellationToken);
            }
        }

        /// <summary>
        /// Unscheduled execution - possibly combine with the SuspendScheduling in options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task ExecuteNow<T>(params object[] args) where T : ScheduledTask
        {
            await ExecuteNow(typeof(T), args);
        }
        /// <summary>
        /// Unscheduled execution - possibly combine with the SuspendScheduling in options
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteNow(Type taskType, params object[] args)
        {
            if (taskType.IsSubclassOf(typeof(ScheduledTask)))
            {
                var t = _scheduledTasks.Find(st => st.Task.GetType() == taskType);
                if (t != null)
                {
                    if (!t.IsRunning)
                    {
                        try
                        {
                            var taskFactory = new TaskFactory(TaskScheduler.Current);
                            await taskFactory.StartNew(
                                () => ExecuteTask(t, ScheduleMode.OnRequest, CancellationToken.None, args)
                                );
                        }
                        catch (OperationCanceledException)
                        {
                            log.Information($"{t.Task.ToString()} cancelled");
                        }
                        catch (Exception xe)
                        {
                            log.Error(xe);
                        }
                    }
                    else
                    {
                        log.Warning($"{t.Task.ToString()} not started as it is already running");
                    }
                }
                else
                {
                    log.Warning($"No task of type {taskType.Name} found");
                }
            }
            else
            {
                log.Error($"{taskType.Name} is not derived from ScheduledTask");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetReport()
        {
            List<string> lines = new List<string>();
            foreach (var item in _scheduledTasks)
            {
                lines.Add(item.ToReportingLine());
            }
            return lines;
        }
        private async Task ExecuteScheduledTasks(CancellationToken cancellationToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var referenceTime = DateTime.UtcNow;

            var tasksThatShouldRun = _scheduledTasks.Where(t => t.ShouldRun(referenceTime)).ToList();

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();
                if (!taskThatShouldRun.IsRunning)
                {
                    if (!taskThatShouldRun.ManualStartOnly)
                    {
                        try
                        {
                            await taskFactory.StartNew(() => ExecuteTask(taskThatShouldRun, ScheduleMode.AtScheduledTime, cancellationToken));
                        }
                        catch (OperationCanceledException)
                        {
                            log.Trace($"{taskThatShouldRun.Task.ToString()} cancelled");
                        }
                        catch (Exception xe)
                        {
                            log.Error(xe);
                            //throw;
                        }
                    }
                    else
                    {
                        log.Information($"{taskThatShouldRun.Task.ToString()} is set to manual only");
                    }
                }
                else
                {
                    log.Warning($"{taskThatShouldRun.Task.ToString()} not started as it is already running, will next start at {taskThatShouldRun.NextRunTime.ToLocalTime().ToDefaultWithTime()}");
                }
            }
        }
        private async Task ExecuteTask(ScheduledTaskWrapper taskWrapper, ScheduleMode mode, CancellationToken cancellationToken, params object[] args)
        {
            try
            {
                if (taskWrapper.Task is ScheduledTask)
                {
                    var task = taskWrapper.Task as ScheduledTask;
                    var count = task.Pipeline.Count();
                    var msg = $"Starting task {task.GetType().Name} [{mode.ToString()}]";
                    if (count > 1)
                    {
                        msg = $"{msg} (pipeline of {task.Pipeline.Count()} items)";
                        //log.LogInformation($"Starting task {task.GetType().Name} (pipeline of {task.Pipeline.Count()} items)");
                    }
                    log.Information(msg);
                }
                var sw = new Stopwatch();
                using (var sm = new SemaphoreSlim(1, 1))
                {
                    await sm.WaitAsync();
                    taskWrapper.IsRunning = true;
                    //taskWrapper.Task.SetMode()
                    sw.Start();
                    await taskWrapper.Task.ExecuteAsync(mode, cancellationToken, args);
                    sw.Stop();
                    taskWrapper.IsRunning = false;
                    log.Information($"Task {taskWrapper.Task.GetType().Name} completed in {sw.Elapsed}, will run next at {taskWrapper.NextRunTime.ToLocalTime().ToDefaultWithTime()}");
                }
            }
            catch (Exception ex)
            {
                var ex_args = new UnobservedTaskExceptionEventArgs(
                    ex as AggregateException ?? new AggregateException(ex));

                UnobservedTaskException?.Invoke(this, ex_args);

                if (!ex_args.Observed)
                {
                    throw;
                }
            }
        }
        private void Initialise()
        {
            //InitialiseRealtimeTasks();
            InitialiseScheduledTasks();
        }
        private void InitialiseScheduledTasks()
        {
            IEnumerable<IScheduledTask> scheduledTasks = sp.GetServices<ScheduledTask>();
            foreach (var st in scheduledTasks)
            {
                var scheduledTask = st as ScheduledTask;
                var serviceSchedule = options.Schedules?.FirstOrDefault(sc => string.Compare(sc.Name, scheduledTask.GetType().Name) == 0);
                if (serviceSchedule == null)
                {
                    log.Error($"{scheduledTask.GetType().Name}, service schedule not found - check SchedulerOptions");
                    continue;
                }
                if (serviceSchedule.Enabled)
                {
                    var referenceTime = DateTime.UtcNow;
                    scheduledTask.Schedule = serviceSchedule.Schedule;
                    var schedule = CrontabSchedule.Parse(scheduledTask.Schedule);
                    var times = schedule.GetNextOccurrences(referenceTime, referenceTime.AddYears(5));
                    if (times.Count() < 2 || (times.Take(1).First() - times.First()).TotalDays > 13 * 7)
                    {
                        log.Error($"{scheduledTask.GetType().Name} schedule not supported: {CrontabSchedule.GetDescription(scheduledTask.Schedule)}");
                    }
                    else
                    {
                        if (scheduledTask.StartAfter > TimeSpan.Zero)
                        {
                            // set next run time to that specified by the user
                            referenceTime += scheduledTask.StartAfter;
                        }
                        else
                        {
                            // set next run time to the second occurrence time
                            // so that it doesn't get run immediately
                            referenceTime = times.Take(1).First();
                        }

                        _scheduledTasks.Add(new ScheduledTaskWrapper
                        {
                            ManualStartOnly = serviceSchedule.ManualStartOnly,
                            Schedule = schedule,
                            Task = scheduledTask,
                            NextRunTime = referenceTime
                        });

                        var msg = $"{scheduledTask.GetType().Name} scheduled: {CrontabSchedule.GetDescription(scheduledTask.Schedule)}";
                        msg = $"{msg}, starts at {referenceTime.ToLocalTime().ToDefaultWithTime()}";
                        if (scheduledTask is ScheduledTask)
                        {
                            log.Information(msg);
                        }
                    }
                }
                else
                {
                    log.Warning($"{scheduledTask.GetType().Name} is not enabled");
                }
            }
            if (options.SuspendScheduling)
            {
                log.Information($"Task scheduling is suspended");
            }
        }
        //private void InitialiseRealtimeTasks()
        //{
        //    realtimeTasks = sp.GetServices<IRealtimeTask>();
        //    log.Debug($"{realtimeTasks.Count()} realtime tasks tasks found");
        //}
        //private void StartRealtimeTasks()
        //{
        //    if (!this.options.SuspendRealtimeTasks)
        //    {
        //        foreach (var rt in realtimeTasks)
        //        {
        //            try
        //            {
        //                Task.Run(async () =>
        //                {
        //                    try
        //                    {
        //                        if (rt.StartAfter > TimeSpan.Zero)
        //                        {
        //                            await Task.Delay(rt.StartAfter);
        //                        }
        //                        await rt.ExecuteAsync(CancellationToken.None);
        //                        log.Debug($"{rt.GetType().Name} started");
        //                    }
        //                    catch (Exception xe)
        //                    {
        //                        log.Error(xe);
        //                        //Debug.WriteLine($"{xe.Message}");
        //                        //Debugger.Break();
        //                        //throw;
        //                    }
        //                });
        //            }
        //            catch (Exception xe)
        //            {
        //                log.Error(xe);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        log.Debug($"Real time tasks are suspended");
        //    }
        //}
    }
}
