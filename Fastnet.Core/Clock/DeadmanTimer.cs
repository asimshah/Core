using System;
using System.Threading;

namespace Fastnet.Core
{
    /// <summary>
    /// A timer to detect a failure
    /// </summary>
    public class DeadmanTimer
    {
        private Timer timer;
        private int initialDelay;
        private int isDeadDelay;
        private Action onDeadInterval;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialDelay">initial detection interval in milliseconds</param>
        /// <param name="isDeadDelay">subsequent detection interval in milliseconds </param>
        /// <param name="onDeadInterval">method to call if Reset() not called in time</param>
        public DeadmanTimer(int initialDelay, int isDeadDelay, Action onDeadInterval)
        {
            this.initialDelay = initialDelay;
            this.isDeadDelay = isDeadDelay;
            this.onDeadInterval = onDeadInterval;
        }
        /// <summary>
        /// call this to start dead man timer
        /// </summary>
        public void Start()
        {
            timer = new Timer(OnTimerFired, null, initialDelay, Timeout.Infinite);
        }
        /// <summary>
        /// call this to start dead man timer
        /// </summary>
        /// <param name="initialDelay"></param>
        /// <param name="isDeadDelay"></param>
        public void Start(int initialDelay, int isDeadDelay)
        {
            this.initialDelay = initialDelay;
            this.isDeadDelay = isDeadDelay;
            Start();
        }
        /// <summary>
        /// call this stop the timer
        /// </summary>
        public void Stop()
        {
            timer = null;
        }
        /// <summary>
        /// keep calling this to prevent dead man firing!
        /// </summary>
        public void Reset()
        {
            if (timer != null)
            {
                timer.Change(isDeadDelay, Timeout.Infinite);
                //timer = new Timer(OnTimerFired, null, isDeadDelay, Timeout.Infinite);
            }
            else
            {
                throw new Exception("Deadman not started!");
            }
        }
        private void OnTimerFired(object state)
        {
            onDeadInterval?.Invoke();
        }
    }
}
