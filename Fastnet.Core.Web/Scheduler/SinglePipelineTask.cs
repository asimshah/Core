using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SinglePipelineTask : ScheduledTask, IPipelineTask

    {
        ///// <summary>
        ///// 
        ///// </summary>
        //public override TimeSpan StartAfter => TimeSpan.Zero;

        /// <summary>
        /// 
        /// </summary>
        public string Name => $"{this.GetType().Name}";
        /// <summary>
        /// 
        /// </summary>
        public TaskMethod ExecuteAsync => DoTask;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerFactory"></param>
        public SinglePipelineTask(ILoggerFactory loggerFactory/*, IOptions<SchedulerOptions> serviceOptionsConfiguration*/) : base(loggerFactory)
        {
            //this.schedulerOptions = serviceOptionsConfiguration.Value;
            //var serviceSchedule = this.schedulerOptions.Schedules?.FirstOrDefault(sc => string.Compare(sc.Name, this.GetType().Name) == 0);
            //schedule = serviceSchedule?.Schedule ?? "0 0 1 */12 *";// default is At 00:00 AM, on day 1 of the month, every 12 months!! not useful!
            this.log = CreatePipelineLogger(this.GetType());
            CreatePipeline(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskState"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected abstract Task<ITaskState> DoTask(ITaskState taskState, ScheduleMode mode, CancellationToken cancellationToken, params object[] args);
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
