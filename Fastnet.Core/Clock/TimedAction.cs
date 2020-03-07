using System;
using System.Diagnostics;

namespace Fastnet.Core
{
    /// <summary>
    /// times the provided action and returns the elapsed time to the OnComplete function
    /// usage: using (new TimedAction((t) =>  { /* t is the elapsed TimeSpan */ })) { /* do something */ }
    /// </summary>
    public class TimedAction : IDisposable
    {
        private readonly Stopwatch sw;
        private readonly Action<TimeSpan> f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OnComplete"></param>
        public TimedAction(Action<TimeSpan> OnComplete)
        {
            this.f = OnComplete;
            sw = Stopwatch.StartNew();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            sw.Stop();
            f(sw.Elapsed);
        }
    }
}
