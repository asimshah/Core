using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fastnet.Core
{
    public class StepTimer
    {
        public Dictionary<string, long> StepTimes { get; set; } = new Dictionary<string, long>();
        private Stopwatch sw;
        public StepTimer(bool startImmediately = false)
        {
            if(startImmediately)
            {
                Start();
            }
        }
        public void Start()
        {
            this.sw = Stopwatch.StartNew();
        }
        public void Stop()
        {
            this.sw.Stop();
        }
        public void Time(string stepName = null)
        {
            if (stepName == null)
            {
                stepName = $"Step {StepTimes.Count() + 1}";
            }
            this.StepTimes.Add(stepName, sw.ElapsedTicks);
        }
        public override string ToString()
        {
            var list = new List<string>();
            int index = 0;
            foreach (var kvp in StepTimes)
            {
                var elapsed = index == 0 ? kvp.Value : StepTimes.ElementAt(index).Value - StepTimes.ElementAt(index - 1).Value;
                var ts = new TimeSpan(elapsed);
                list.Add($"{kvp.Key}: {ts.TotalMilliseconds.ToString("#0")}ms");
                ++index;
            }
            return string.Join(", ", list);
        }
    }
}
