
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Fastnet.Core.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public static class RollingFileLoggerExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="level"></param>
        /// <param name="text"></param>
        public static void Log(this ILogger logger, LogLevel level, string text)
        {
            switch (level)
            {
                default:
                case LogLevel.None:
                    break;
                case LogLevel.Critical:
                    logger.LogCritical(text);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug(text);
                    break;
                case LogLevel.Error:
                    logger.LogError(text);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(text);
                    break;
                case LogLevel.Trace:
                    logger.LogTrace(text);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(text);
                    break;
            }
        }
        /// <summary>
        /// Adds a Rolling File log to the Logging infrastructure
        /// (alternatively use AddWebRollingFile in an aspnetcore app)
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ILoggingBuilder AddRollingFile(this ILoggingBuilder builder)
        {

            builder.Services.AddSingleton<ILoggerProvider, RollingFileLoggerProvider>();
            return builder;
        }
        /// <summary>
        /// Adds a rolling file logger
        /// (consider using the ILoggingBuilder extension instead)
        /// </summary>
        public static ILoggerFactory AddRollingFile(this ILoggerFactory factory)
        {
            return factory.AddRollingFile(includeScopes: false);
        }

#pragma warning disable CS0419 // Ambiguous reference in cref attribute
        /// <summary>
        /// Adds a rolling file logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// (consider using the ILoggingBuilder extension instead)
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddRollingFile(this ILoggerFactory factory, bool includeScopes)
#pragma warning restore CS0419 // Ambiguous reference in cref attribute
        {
            factory.AddRollingFile((n, l) => l >= LogLevel.Information, includeScopes);
            return factory;
        }
#pragma warning disable CS0419 // Ambiguous reference in cref attribute
        /// <summary>
        /// Adds a rolling file logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// (consider using the ILoggingBuilder extension instead)
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        public static ILoggerFactory AddRollingFile(this ILoggerFactory factory, LogLevel minLevel)
#pragma warning restore CS0419 // Ambiguous reference in cref attribute
        {
            factory.AddRollingFile(minLevel, includeScopes: false);
            return factory;
        }
#pragma warning disable CS0419 // Ambiguous reference in cref attribute
        /// <summary>
        /// Adds a rolling file logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// (consider using the ILoggingBuilder extension instead)
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddRollingFile(this ILoggerFactory factory, LogLevel minLevel, bool includeScopes)
#pragma warning restore CS0419 // Ambiguous reference in cref attribute
        {
            factory.AddRollingFile((category, logLevel) => logLevel >= minLevel, includeScopes);
            return factory;
        }
        /// <summary>
        /// Adds a rolling file logger that is enabled as defined by the filter function.
        /// (consider using the ILoggingBuilder extension instead)
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="filter"></param>
        public static ILoggerFactory AddRollingFile(this ILoggerFactory factory, Func<string, LogLevel, bool> filter)
        {
            factory.AddRollingFile(filter, includeScopes: false);
            return factory;
        }
        /// <summary>
        /// Adds a rolling file logger that is enabled as defined by the filter function.
        /// (consider using the ILoggingBuilder extension instead)
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="filter"></param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output. (not implemented!)</param>
        public static ILoggerFactory AddRollingFile(this ILoggerFactory factory, Func<string, LogLevel, bool> filter, bool includeScopes)
        {
            factory.AddProvider(new RollingFileLoggerProvider(filter, includeScopes));
            return factory;
        }
        /// <summary>
        /// Adds a rolling file logger that is enabled as defined in settings.
        /// (consider using the ILoggingBuilder extension instead)
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static ILoggerFactory AddRollingFile(this ILoggerFactory factory, IRollingFileLoggerSettings settings)
        {
            factory.AddProvider(new RollingFileLoggerProvider(settings));
            return factory;
        }
        /// <summary>
        /// Adds a rolling file logger that is enabled as defined by configuration
        /// (consider using the ILoggingBuilder extension instead)
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ILoggerFactory AddRollingFile(this ILoggerFactory factory, IConfiguration configuration)
        {
            var settings = new ConfigurationRollingFileLoggerSettings(configuration);

            return factory.AddRollingFile(settings);
        }
    }
}
