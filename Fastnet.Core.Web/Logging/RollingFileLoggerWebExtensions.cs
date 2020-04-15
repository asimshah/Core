using Fastnet.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Fastnet.Core.Web.Logging
{
    //public static class RollingFileLoggerWebExtensions
    //{
    //    /// <summary>
    //    /// Adds a rolling file log to the Logging infrastructure
    //    /// </summary>
    //    /// <param name="builder"></param>
    //    /// <returns></returns>
    //    [Obsolete("use ILoggingBuilder AddRollingFile instead")]
    //    public static ILoggingBuilder AddWebRollingFile(this ILoggingBuilder builder)
    //    {
    //        //builder.Services.AddSingleton<ILoggerProvider, RollingFileLoggerWebProvider>();
    //        builder.Services.AddSingleton<ILoggerProvider, RollingFileLoggerProvider>();
    //        return builder;
    //    }
    //    /// <summary>
    //    /// Adds a rolling file logger
    //    /// (consider using the ILoggingBuilder extension instead)
    //    /// </summary>
    //    /// <param name="factory"></param>
    //    /// <param name="sp"></param>
    //    /// <returns></returns>
    //    [Obsolete("use ILoggingBuilder AddRollingFile instead")]
    //    public static ILoggerFactory AddWebRollingFile(this ILoggerFactory factory, IServiceProvider sp)
    //    {
    //        return factory.AddWebRollingFile(sp, includeScopes: false);
    //    }
    //    /// <summary>
    //    /// Adds a rolling file logger
    //    /// </summary>
    //    /// <param name="factory"></param>
    //    /// <param name="sp"></param>
    //    /// <param name="includeScopes"></param>
    //    /// <returns></returns>
    //    [Obsolete("use ILoggingBuilder AddRollingFile instead")]
    //    public static ILoggerFactory AddWebRollingFile(this ILoggerFactory factory, IServiceProvider sp, bool includeScopes)
    //    {
    //        factory.AddWebRollingFile(sp, (n, l) => l >= LogLevel.Information, includeScopes);
    //        return factory;
    //    }
    //    /// <summary>
    //    /// Adds a rolling file logger that is enabled for LogLevels of minLevel or higher.
    //    /// </summary>
    //    /// <param name="factory"></param>
    //    /// <param name="sp"></param>
    //    /// <param name="minLevel"></param>
    //    /// <returns></returns>
    //    [Obsolete("use ILoggingBuilder AddRollingFile instead")]
    //    public static ILoggerFactory AddWebRollingFile(this ILoggerFactory factory, IServiceProvider sp, LogLevel minLevel)
    //    {
    //        factory.AddWebRollingFile(sp, minLevel, includeScopes: false);
    //        return factory;
    //    }
    //    /// <summary>
    //    /// Adds a rolling file logger that is enabled for LogLevels of minLevel or higher.
    //    /// </summary>
    //    /// <param name="factory"></param>
    //    /// <param name="sp"></param>
    //    /// <param name="minLevel">The minimum LogLevel to be logged</param>
    //    /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
    //    /// in the output.</param>
    //    [Obsolete("use ILoggingBuilder AddRollingFile instead")]
    //    public static ILoggerFactory AddWebRollingFile(this ILoggerFactory factory, IServiceProvider sp, LogLevel minLevel, bool includeScopes)
    //    {
    //        factory.AddWebRollingFile(sp, (category, logLevel) => logLevel >= minLevel, includeScopes);
    //        return factory;
    //    }
    //    /// <summary>
    //    /// Adds a rolling file logger that is enabled as defined by the filter function.
    //    /// </summary>
    //    /// <param name="factory"></param>
    //    /// <param name="sp"></param>  
    //    /// <param name="filter"></param>

    //    [Obsolete("use ILoggingBuilder AddRollingFile instead")]
    //    public static ILoggerFactory AddWebRollingFile(this ILoggerFactory factory, IServiceProvider sp, Func<string, LogLevel, bool> filter)
    //    {
    //        factory.AddWebRollingFile(sp, filter, includeScopes: false);
    //        return factory;
    //    }
    //    /// <summary>
    //    /// Adds a rolling file logger that is enabled as defined by the filter function.
    //    /// </summary>
    //    /// <param name="factory"></param>
    //    /// <param name="sp"></param>
    //    /// <param name="filter"></param>
    //    /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
    //    /// in the output. (not implemented!)</param>
    //    [Obsolete("use ILoggingBuilder AddRollingFile instead")]
    //    public static ILoggerFactory AddWebRollingFile(this ILoggerFactory factory, IServiceProvider sp, Func<string, LogLevel, bool> filter, bool includeScopes)
    //    {
    //        factory.AddProvider(new RollingFileLoggerWebProvider(sp, filter, includeScopes));
    //        return factory;
    //    }
    //    /// <summary>
    //    /// Adds a rolling file logger that is enabled as defined in settings.
    //    /// </summary>
    //    /// <param name="factory"></param>
    //    /// <param name="sp"></param>
    //    /// <param name="settings"></param>
    //    /// <returns></returns>
    //    [Obsolete("use ILoggingBuilder AddRollingFile instead")]
    //    internal static ILoggerFactory AddWebRollingFile(this ILoggerFactory factory, IServiceProvider sp, IRollingFileLoggerSettings settings)
    //    {
    //        factory.AddProvider(new RollingFileLoggerWebProvider(sp, settings));
    //        return factory;
    //    }
    //}
}
