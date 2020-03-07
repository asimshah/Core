using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Fastnet.Core.Logging
{
    /// <summary>
    /// Use this create loggers when Dependency Injection is not available
    /// </summary>
    public static class ApplicationLoggerFactory
    {
        private static IServiceProvider serviceProvider;
        /// <summary>
        /// Initialises static ILogger creation
        /// </summary>
        /// <param name="sp"></param>
        public static void Init(IServiceProvider sp)
        {
            serviceProvider = sp;
        }
        /// <summary>
        /// Create a logger categorised by T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger CreateLogger<T>()
        {
            if (serviceProvider != null)
            {
                var lf = serviceProvider.GetService<ILoggerFactory>();
                return lf.CreateLogger<T>();
            }
            Debug.WriteLine($"IServiceProvider is null - valid if called without ApplicationLogFactory.Init(), e.g. during add-migration");
            return null;
            //throw new Exception($"IServiceProvider is null - ApplicationLogFactory.Init() not called?");
        }
        /// <summary>
        /// Create a logger categorised by name
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public static ILogger CreateLogger(string categoryName)
        {
            if (serviceProvider != null)
            {
                var lf = serviceProvider.GetService<ILoggerFactory>();
                return lf.CreateLogger(categoryName);
            }
            Debug.WriteLine($"IServiceProvider is null - valid if called without ApplicationLogFactory.Init(), e.g. during add-migration");
            return null;
        }
        /// <summary>
        /// Create a logger categorised by Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ILogger CreateLogger(Type type)
        {
            if (serviceProvider != null)
            {
                var lf = serviceProvider.GetService<ILoggerFactory>();
                return lf.CreateLogger(type);
            }
            Debug.WriteLine($"IServiceProvider is null - valid if called without ApplicationLogFactory.Init(), e.g. during add-migration");
            return null;
        }
    }
}
