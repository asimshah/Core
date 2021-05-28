using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{

    /// <summary>
    /// 
    /// </summary>
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Add a (singleton) background service of type T. Such services are started/stopped automatically but
        /// are also accessible using injection (unlile AddHostedService() !!)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddService<T>(this IServiceCollection services) where T : HostedService
        {
            services.AddSingleton<T>();
            return services.AddSingleton<IHostedService>(sp =>
            {
                var instance = sp.GetService<T>();
                if (instance == null)
                {
                    Debug.WriteLine($"Service instance of type {typeof(T)} not found");
                }
                return instance;
            });
        }
        /// <summary>
        /// Adds the Scheduler to the services collection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddScheduler(this IServiceCollection services, IConfiguration config)
        {
            
            try
            {
                services.Configure<SchedulerOptions>(config.GetSection("SchedulerOptions"));
                return services.AddService<SchedulerService>();

            }
            catch (Exception xe)
            {
                Debug.WriteLine($"Exception: {xe.Message}");
                throw;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        [Obsolete("use injection instead")]
        public static SchedulerService GetSchedulerService(this IServiceProvider sp)
        {
            return sp.GetService<IHostedService>() as SchedulerService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static T GetScheduledTask<T>(this IServiceProvider sp) where T : ScheduledTask
        {
            return sp.GetServices<ScheduledTask>().SingleOrDefault(x => x is T) as T;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        [Obsolete("derive from HostedService and use IServiceCollection.AddService instead")]
        public static T GetRealtimeTask<T>(this IServiceProvider sp) where T : RealtimeTask
        {
            return sp.GetServices<RealtimeTask>().SingleOrDefault(x => x is T) as T;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="RT"></typeparam>
        /// <param name="ss"></param>
        /// <returns></returns>
        [Obsolete("derive from HostedService and use IServiceCollection.AddService instead")]
        public static RT GetRealtimeTask<RT>(this SchedulerService ss) where RT : RealtimeTask
        {
            return null;// ss.realtimeTasks.SingleOrDefault(t => t is RT) as RT;
        }
    }

}
