namespace Fastnet.Core.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class SchedulerOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public bool TraceTaskLifeCycle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool SuspendScheduling { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool SuspendRealtimeTasks { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool TraceTaskPolling { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool DebugBreak { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ServiceSchedule[] Schedules { get; set; }
    }
}
