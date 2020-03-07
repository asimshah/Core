using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{
    /// <summary>
    /// The method that caused execution to start
    /// </summary>
    public enum ScheduleMode
    {
        /// <summary>
        /// Execution took place according to schedule
        /// </summary>
        AtScheduledTime,
        /// <summary>
        /// Execution took place by request
        /// </summary>
        OnRequest
    }
    /// <summary>
    /// 
    /// </summary>
    public interface IPipelineTask
    {
        /// <summary>
        /// 
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 
        /// </summary>
        TaskMethod ExecuteAsync { get; }
    }

    /// <summary>
    /// A task to run on a schedule.
    /// using the abstract ScheduledTask type is preferable
    /// </summary>
    internal interface IScheduledTask //: ITask
    {
        TimeSpan StartAfter { get; } 
        //string Schedule { get; }
        Task ExecuteAsync(ScheduleMode mode, CancellationToken cancellationToken, params object[] args);
        Func<ScheduleMode, Task> BeforeTaskStartsAsync { get; }
        Func<ScheduleMode, Task> AfterTaskCompletesAsync { get; }
    }
    /// <summary>
    /// Called by the scheduler service to execute the task.
    /// </summary>
    /// <param name="taskState">only used in pipelined tasks</param>
    /// <param name="mode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate Task<ITaskState> TaskMethod(ITaskState taskState, ScheduleMode mode, CancellationToken cancellationToken, params object[] args);
    ///// <summary>
    ///// Called by the scheduler service before the task starts
    ///// </summary>
    ///// <param name="mode"></param>
    ///// <returns></returns>
    //public delegate Task TaskBeginningMethod(ScheduleMode mode);
    ///// <summary>
    ///// Called by the scheduler service after the task (i.e. the complete pipeline) has finished
    ///// </summary>
    ///// <param name="mode">The mode in which execution had occurred</param>
    ///// <returns></returns>
    //public delegate Task TaskCompletionMethod(ScheduleMode mode);

}

