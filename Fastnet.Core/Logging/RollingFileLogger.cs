using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("Fastnet.Core.Web")]
namespace Fastnet.Core.Logging
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class RollingFileLogger : ILogger

    {
        private readonly string[] lineIndicator = new string[] { ", @line " };
        private FileWriter fw;
        private Func<string, LogLevel, bool> _filter;
        public string Name { get; }
        public bool IncludeScopes { get; set; }
        public Func<string, LogLevel, bool> Filter
        {
            get { return _filter; }
            set
            {
                _filter = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        internal RollingFileLogger(string logFolder, string name, Func<string, LogLevel, bool> filter, bool includeScopes)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Filter = filter ?? ((category, logLevel) => true);
            IncludeScopes = includeScopes;
            fw = FileWriter.Get(logFolder);
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
            {
                return false;
            }
            return Filter(Name, logLevel);
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, Name, eventId.Id, message, exception);
            }
        }
        //internal void WriteText(string text)
        //{
        //    fw.WriteText(text);
        //}
        protected void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            //?? insert eventId into log text??
            if (IncludeScopes)
            {
                //ignored for now
            }
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;

            var parts = message.Split(lineIndicator, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                message = parts[0];
                logName = logName + parts[1];
            }
            fw.WriteMessage(logLevel, logName, processId, Thread.CurrentThread.ManagedThreadId, message, exception);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return RollingFileLogScope.Push(Name, state);
        }
        //private void GetScopeInformation(StringBuilder builder)
        //{
        //    var current = RollingFileLogScope.Current;
        //    string scopeLog = string.Empty;
        //    var length = builder.Length;

        //    while (current != null)
        //    {
        //        if (length == builder.Length)
        //        {
        //            scopeLog = $"=> {current}";
        //        }
        //        else
        //        {
        //            scopeLog = $"=> {current} ";
        //        }

        //        builder.Insert(length, scopeLog);
        //        current = current.Parent;
        //    }
        //    if (builder.Length > length)
        //    {
        //        builder.Insert(length, _messagePadding);
        //        builder.AppendLine();
        //    }
        //}
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
