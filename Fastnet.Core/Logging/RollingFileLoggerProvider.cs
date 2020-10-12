using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace Fastnet.Core.Logging
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class RollingFileLoggerProvider : ILoggerProvider

    {
        protected readonly ConcurrentDictionary<string, RollingFileLogger> _loggers = new ConcurrentDictionary<string, RollingFileLogger>();
        protected readonly Func<string, LogLevel, bool> _filter;
        protected IRollingFileLoggerSettings _settings;
        private static readonly Func<string, LogLevel, bool> trueFilter = (cat, level) => true;
        private static readonly Func<string, LogLevel, bool> falseFilter = (cat, level) => false;
        private IDisposable _optionsReloadToken;
        private bool _includeScopes;
        //private string webRootPath;
        private string logFolderPath;
        public RollingFileLoggerProvider(Func<string, LogLevel, bool> filter, bool includeScopes)
        {
            //_env = env;
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            //_settings = new RollingFileLoggerSettings()
            //{
            //    LogFolder = "logs",
            //    AppFolder = null,
            //    IncludeScopes = includeScopes
            //};
            var path = Path.Combine(Environment.CurrentDirectory, "logs");
            if(Directory.Exists(path))
            {
                logFolderPath = path;
            }
            else
            {
                logFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            //Debug.WriteLine($"Rolling file log path is {logFolderPath}");
            _includeScopes = includeScopes;
        }
        /// <summary>
        /// This is the constructor that my web sites use ...
        /// </summary>
        /// <param name="options"></param>
        /// <param name="env"></param>
        public RollingFileLoggerProvider(IOptionsMonitor<RollingFileLoggerOptions> options, IHostEnvironment env)
        {
            // *NB* not sure what ContentRootPath is in the case of a non-aspnetcore app (such as a console app)
            // it may be that the folder is not writable easily (such as if it is in ProgramFiles, for example)
            // an alternative is to use the older technique of creating a folder using
            // something like Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            // BUT I am not clear how to detect this case (IWebHostEnvironment is not available here...)
            // for now - 31Oct2019 - I am ignoring this issue as I do not have a need for a non AspNetCore app in the near future
            switch(options.CurrentValue.LogFolderSetting)
            {
                case LogFolderSetting.Normal:
                    logFolderPath = env.ContentRootPath;
                    break;
                //case LogFolderSetting.Special:
                //    logFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //    break;
                case LogFolderSetting.Custom:
                    logFolderPath = options.CurrentValue.LogFolder;
                    break;
            }

            _filter = trueFilter;
            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            ReloadLoggerOptions(options.CurrentValue);
        }
        public RollingFileLoggerProvider(IRollingFileLoggerSettings settings)
        {
            //_env = env;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            if (_settings.ChangeToken != null)
            {
                _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }
        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, CreateLoggerImplementation);
        }
        protected virtual RollingFileLogger CreateLoggerImplementation(string name)
        {
            var includeScopes = _settings?.IncludeScopes ?? _includeScopes;
            if(string.IsNullOrWhiteSpace(logFolderPath))
            {
                Debugger.Break();
            }
            string logFolder = Path.Combine(logFolderPath, "logs");
            //Debug.WriteLine($"log folder is {logFolder}");
            return new RollingFileLogger(logFolder, name, GetFilter(name, _settings), includeScopes);

        }
        //protected RollingFileLogger CreateLoggerImplementation(string name, string basePath)
        //{
        //    if(webRootPath != null)
        //    {
        //        // always use the webrootpath if available
        //        basePath = webRootPath;
        //    }
        //    if (!Directory.Exists(basePath))
        //    {
        //        Directory.CreateDirectory(basePath);
        //    }
        //    var includeScopes = _settings?.IncludeScopes ?? _includeScopes;
        //    string logFolder = Path.Combine(basePath, "logs");
        //    Debug.WriteLine($"log folder is {logFolder}");
        //    return new RollingFileLogger(logFolder, name, GetFilter(name, _settings), includeScopes);
        //}
        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }
        // *NB* keep this method as I may need it in futire - see comment in
        // RollingFileLoggerProvider(IOptionsMonitor<RollingFileLoggerOptions> options, IHostEnvironment env)
        private string GetExecutableName()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
            //return "something";
        }
        private Func<string, LogLevel, bool> GetFilter(string name, IRollingFileLoggerSettings settings)
        {
            if (_filter != null)
            {
                return _filter;
            }

            if (settings != null)
            {
                foreach (var prefix in GetKeyPrefixes(name))
                {
                    LogLevel level;
                    if (settings.TryGetSwitch(prefix, out level))
                    {
                        return (n, l) => l >= level;
                    }
                }
            }

            return (n, l) => false;
        }
        private IEnumerable<string> GetKeyPrefixes(string name)
        {
            while (!string.IsNullOrEmpty(name))
            {
                yield return name;
                var lastIndexOfDot = name.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    yield return "Default";
                    break;
                }
                name = name.Substring(0, lastIndexOfDot);
            }
        }
        private void OnConfigurationReload(object state)
        {
            try
            {
                // The settings object needs to change here, because the old one is probably holding on
                // to an old change token.
                _settings = _settings.Reload();
                var includeScopes = _settings?.IncludeScopes ?? false;
                foreach (var logger in _loggers.Values)
                {
                    logger.Filter = GetFilter(logger.Name, _settings);
                    logger.IncludeScopes = includeScopes;

                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error while loading configuration changes.{Environment.NewLine}{ex}");
            }
            finally
            {
                // The token will change each time it reloads, so we need to register again.
                if (_settings?.ChangeToken != null)
                {
                    _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
                }
            }
        }
        private void ReloadLoggerOptions(RollingFileLoggerOptions options)
        {
            _includeScopes = options.IncludeScopes;
            foreach (var logger in _loggers.Values)
            {
                logger.IncludeScopes = _includeScopes;
            }
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
