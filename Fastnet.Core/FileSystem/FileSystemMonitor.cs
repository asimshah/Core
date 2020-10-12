using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class FileSystemMonitorOptions
    {
        /// <summary>
        /// Interval in milliseconds after changes start and have stopped, default = 5000
        /// </summary>
        public int ChangeNotificationIdle { get; set; }
        /// <summary>
        /// Interval in milliseconds at which access to the path is checked, default = 500
        /// </summary>
        public int PathAvailabilityMonitorInterval { get; set; }
        /// <summary>
        /// Records changes in access to the path being monitored
        /// </summary>
        public bool LogPathAvailabilityChanges { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public FileSystemMonitorOptions()
        {
            this.ChangeNotificationIdle = 5000;
            this.PathAvailabilityMonitorInterval = 500;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FileSystemMonitorFactory
    {
        private readonly ILoggerFactory lf;
        private readonly FileSystemMonitorOptions options;
        /// <summary>
        /// 
        /// </summary>
        public FileSystemMonitorFactory(ILoggerFactory lf, IOptions<FileSystemMonitorOptions> options)
        {
            this.lf = lf;
            this.options = options.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onChanges"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public FileSystemMonitor CreateMonitor(string path, Action<IEnumerable<FileSystemMonitorEvent>> onChanges, Action<Exception> onError = null)
        {
            return new FileSystemMonitor(this.lf.CreateLogger<FileSystemMonitor>(), path, this.options, onChanges, onError);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FileSystemMonitorEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public WatcherChangeTypes Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OldPath { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FileSystemMonitor : FileSystemWatcher
    {
        private DateTimeOffset lastChangeTime;
        private readonly ILogger log;
        private readonly ConcurrentQueue<FileSystemMonitorEvent> queue;
        private CancellationTokenSource onChangesCancellationSource;
        private CancellationTokenSource pathMonitorCancellationSource;
        private readonly Action<Exception> onError;
        private bool isPathReachable;
        private readonly string path;
        private readonly FileSystemMonitorOptions options;
        private readonly Action<IEnumerable<FileSystemMonitorEvent>> onChanges;
        private bool IsPathReachable
        {
            get => isPathReachable;
            set
            {
                var oldval = isPathReachable;
                isPathReachable = value;
                if (oldval != this.IsPathReachable)
                {
                    PathReachabilityChanged();
                }
            }
        }
        internal FileSystemMonitor(ILogger log, string path, FileSystemMonitorOptions options, Action<IEnumerable<FileSystemMonitorEvent>> onChanges,
             Action<Exception> onError) : base(path)
        {
            this.log = log;
            this.queue = new ConcurrentQueue<FileSystemMonitorEvent>();

            this.options = options;
            this.path = path;
            this.onChanges = onChanges ?? delegate { };
            this.onError = onError ?? delegate { };

        }
        /// <summary>
        /// Starts monitoring the file system path
        /// </summary>
        public void Start()
        {
            Initialise();
            MonitorPath(); // Note: EnableRaisingEvents will set on the path being found
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            this.EnableRaisingEvents = false;
            if (this.pathMonitorCancellationSource != null)
            {
                this.pathMonitorCancellationSource.Cancel();
            }
            if (this.onChangesCancellationSource != null)
            {
                this.onChangesCancellationSource.Cancel();
            }
            this.DetachEvents();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            Stop();
            base.Dispose(disposing);
        }
        private void MonitorPath()
        {
            try
            {
                this.IsPathReachable = false;
                this.pathMonitorCancellationSource = new CancellationTokenSource();
                Task.Run(async () =>
                {
                    try
                    {
                        while (!pathMonitorCancellationSource.Token.IsCancellationRequested)
                        {
                            IsPathReachable = Directory.Exists(path);
                            await Task.Delay(this.options.PathAvailabilityMonitorInterval, pathMonitorCancellationSource.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        log.Information($"Path {Path}, path monitoring task cancelled");
                    }
                    catch (Exception xe)
                    {
                        log.Error(xe);
                    }
                });
            }
            catch (Exception xe)
            {
                log.Error(xe);
                throw;
            }
        }
        private void Initialise()
        {
            lastChangeTime = DateTimeOffset.Now;
            onChangesCancellationSource = new CancellationTokenSource();
            try
            {
                Task.Run(async () =>
                {
                    try
                    {
                        //var pulseCount = 0;
                        while (!onChangesCancellationSource.Token.IsCancellationRequested)
                        {
                            await Task.Delay(2000);
                            //onChangesCancellationSource.Token.ThrowIfCancellationRequested();
                            var targetTime = lastChangeTime + TimeSpan.FromMilliseconds(this.options.ChangeNotificationIdle); // default is 5000 ms
                            log.Trace($"{DateTimeOffset.Now.ToDefaultWithTime()}: queue count {queue.Count}, next update at {targetTime.ToDefaultWithTime()}");
                            if (!queue.IsEmpty && DateTimeOffset.Now > targetTime)
                            {
                                var list = new List<FileSystemMonitorEvent>();
                                while (!queue.IsEmpty)
                                {
                                    if (queue.TryDequeue(out FileSystemMonitorEvent fse))
                                    {
                                        list.Add(fse);
                                    }
                                }
                                onChanges(list);
                                lastChangeTime = DateTimeOffset.Now;
                            }

                        }
                    }
                    catch (OperationCanceledException)
                    {
                        log.Information($"{Path} change buffering task cancelled");
                    }
                    catch (Exception xe)
                    {
                        log.Error(xe);
                    }
                });
            }
            catch (Exception xe)
            {
                log.Error(xe);
            }
            this.AttachEvents();
        }
        private void AttachEvents()
        {
            this.Changed += ChangeEvent;
            this.Created += ChangeEvent;
            this.Deleted += ChangeEvent;
            this.Renamed += ChangeEvent;
            this.Error += ErrorEvent;
        }
        private void DetachEvents()
        {

            this.Changed -= ChangeEvent;
            this.Created -= ChangeEvent;
            this.Deleted -= ChangeEvent;
            this.Renamed -= ChangeEvent;
            this.Error -= ErrorEvent;
        }
        private void ErrorEvent(object sender, ErrorEventArgs e)
        {
            var xe = e.GetException();
            log.Error(xe);
            this.onError(xe);
        }
        private void ChangeEvent(object sender, FileSystemEventArgs ev)
        {
            //if (!ev.FullPath.StartsWith("$") && !ev.FullPath.EndsWith(".probe.txt"))
            if(ev.Name.StartsWith("$") && ev.Name.EndsWith(".probe.txt"))
            {
                return;
            }
            log.Debug($"{ev.ChangeType.ToString()}, {ev.FullPath}");
            lastChangeTime = DateTimeOffset.Now;
            switch (ev)
            {
                case RenamedEventArgs e:
                    queue.Enqueue(new FileSystemMonitorEvent { Type = e.ChangeType, Path = e.FullPath, OldPath = e.OldFullPath });// (rea.ChangeType, rea.FullPath, rea.OldFullPath));
                    break;
                case FileSystemEventArgs e:
                    queue.Enqueue(new FileSystemMonitorEvent { Type = e.ChangeType, Path = e.FullPath, OldPath = null });
                    break;
            }
        }

        private void PathReachabilityChanged()
        {
            if (options.LogPathAvailabilityChanges)
            {
                log.Information($"Path {path} is {(IsPathReachable ? "reachable" : "not reachable")}");
            }
            if (this.IsPathReachable)
            {
                // was unavailable
                this.EnableRaisingEvents = true;
            }
            else
            {
                //was available
                this.EnableRaisingEvents = false;
            }
        }
    }
}
