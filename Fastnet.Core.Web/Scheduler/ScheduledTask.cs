using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{

    /// <summary>
    /// Base for a pipeline of tasks on a schedule
    /// The pipeline will start at the specified schedule and each job in the pipeline
    /// will run in sequence. Pipelined jobs can return an ITaskState which wil lbe passed to
    /// the next job in line. (the first job in the pipeline receives a null ITaskState)
    /// </summary>
    public abstract class ScheduledTask : IScheduledTask // ScheduledTaskInternal
    {
        //private ITaskState savedInitialState;
        /// <summary>
        /// 
        /// </summary>
        protected readonly ILoggerFactory loggerFactory;
        private bool SingleItemPipeline { get => Pipeline.Count == 1; }
        /// <summary>
        /// 
        /// </summary>
        protected ILogger log;
        internal List<IPipelineTask> Pipeline { get; set; }
        /// <summary>
        /// Interval by which the schedule should be delayed
        /// </summary>
        //public abstract TimeSpan StartAfter { get; }
        public TimeSpan StartAfter { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// Cron string defining the schedule
        /// </summary>
        internal string Schedule { get; set; }
        /// <summary>
        /// Optional method to call before the task starts
        /// </summary>
        public Func<ScheduleMode, Task> BeforeTaskStartsAsync { get; set; }
        /// <summary>
        /// Optional method to call after the task completes (i.e. the complete pipeline)
        /// </summary>
        public Func<ScheduleMode, Task> AfterTaskCompletesAsync { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerFactory">provided by Dependency injection</param>
        public ScheduledTask(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            log = loggerFactory.CreateLogger(this.GetType().FullName);
            //savedInitialState = initialState;
            Pipeline = new List<IPipelineTask>();
        }
        async Task IScheduledTask.ExecuteAsync(ScheduleMode mode, CancellationToken cancellationToken, params object[] args)
        {
            if (BeforeTaskStartsAsync != null)
            {
                try
                {
                    await BeforeTaskStartsAsync(mode);
                    //await BeforeTaskStartsAsync.Invoke(mode);
                }
                catch (Exception xe)
                {
                    log.Error(xe, $"BeforeTaskStartsAsync failed");
                }
            }
            if (Pipeline.Count > 0)
            {
                ITaskState state = null;
                foreach (var f in Pipeline)
                {
                    try
                    {
                        Stopwatch sw = new Stopwatch();
                        log.Debug($"starting {f.Name} {mode.ToString()} [from {nameof(ScheduledTask)}]");
                        sw.Start();
                        state = await f.ExecuteAsync(state, mode, cancellationToken, args);
                        sw.Stop();
                        log.Debug($"{f.Name} completed in {sw.Elapsed.ToString()}  [from {nameof(ScheduledTask)}]");
                    }
                    catch (System.Exception xe)
                    {
                        log.Error(xe, $"Pipeline item {f.Name} failed");
                    }
                }
                if (AfterTaskCompletesAsync != null)
                {
                    try
                    {
                        await AfterTaskCompletesAsync(mode);
                        //await AfterTaskCompletesAsync.Invoke(mode);
                    }
                    catch (Exception xe)
                    {
                        log.Error(xe, $"AfterTaskCompletesAsync failed");
                    }
                }
            }
            else
            {
                log.Warning($"Task pipeline is empty");
            }
            return;
        }
        /// <summary>
        /// create a new pipeline if IPipelineTask items which will be run sequentially
        /// Note the first item will receive the initial ITaskState (null be default) and
        /// subsequent items will receive the state returned by the previous item
        /// </summary>
        /// <param name="items"></param>
        protected void CreatePipeline(IEnumerable<IPipelineTask> items)
        {
            Pipeline.Clear();
            if (items.Count() > 0)
            {
                Pipeline.AddRange(items);
            }
            if (!SingleItemPipeline)
            {
                foreach (var item in Pipeline)
                {
                    log.Debug($"{item.Name} added to pipeline");
                }
            }

        }
        /// <summary>
        /// create a new pipeline containing a single item
        /// Note this item will receive the initial ITaskState (null be default)
        /// </summary>
        /// <param name="item"></param>
        protected void CreatePipeline(IPipelineTask item)
        {
            CreatePipeline(new IPipelineTask[] { item });
        }
        /// <summary>
        /// create a standard ILogger for <typeparamref name="T"/> as a <see cref="IPipelineTask"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected ILogger CreatePipelineLogger<T>() where T : IPipelineTask
        {
            return this.CreatePipelineLogger(typeof(T));
        }
        /// <summary>
        /// create a standard ILogger using a name, as a <see cref="IPipelineTask"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected ILogger CreatePipelineLogger(string name)
        {
            return this.loggerFactory.CreateLogger($"{this.GetType().FullName}-{name}");
        }
        /// <summary>
        /// create a standard ILogger using a Type, as a <see cref="IPipelineTask"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected ILogger CreatePipelineLogger(Type type)
        {
            return CreatePipelineLogger(type.Name);
        }

    };

}

