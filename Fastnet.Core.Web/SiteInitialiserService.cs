using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{
    /// <summary>
    /// 
    /// </summary>
    public static class _sisextensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInitialiser<T>(this IServiceCollection services) where T : SiteInitialiserService
        {
            return services.AddHostedService<T>();
        }
    }
    /// <summary>
    /// Arguments provided to the OnComplete method, if specified
    /// </summary>
    /// <typeparam name="T">EFCore db context derived from DbContext</typeparam>
    /// <param name="Database">current instance of the database</param>
    /// <param name="Migrated">true if a database migration was performed</param>
    /// <param name="ServiceScope">use this scope for any service requests</param>
    public record SiteInitialiserCompletedArgs<T>(T Database, bool Migrated, IServiceScope ServiceScope) where T: DbContext;
    /// <summary>
    /// This class can be used a base for a hosted service that can perform site initialisation.
    /// EFCore database migration is available as a built-in method. Further actions are possible
    /// within the same servoce scope by defining an OnComplete parameter to InitialiseDatabaseAsync
    /// </summary>
    public abstract class SiteInitialiserService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        /// <summary>
        /// 
        /// </summary>
        protected readonly ILogger log;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="log"></param>
        public SiteInitialiserService(IServiceProvider serviceProvider, ILogger log)
        {
            this.serviceProvider = serviceProvider;
            this.log = log;
        }
        /// <summary>
        /// Override this method and call InitialiseDatabaseAsync for each database
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        /// <summary>
        /// Initialises the database - currently runs EFcore migratio if required. Calls OnComplete after this migration (if this method is defined)
        /// (should a separate seed method also be available? can be done inside OnComplete)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OnComplete"></param>
        /// <returns></returns>
        protected async Task InitialiseDatabaseAsync<T>(Func<SiteInitialiserCompletedArgs<T>, Task> OnComplete = null) where T : DbContext
        {
            log.Information($"{nameof(InitialiseDatabaseAsync)} for {typeof(T).Name} started");
            using (var scope = serviceProvider.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetService<T>();
                    var migrated = await MigrateAsync(db);
                    await OnComplete?.Invoke(new SiteInitialiserCompletedArgs<T>(db, migrated, scope));
                }
                catch (System.Exception xe)
                {
                    log.Error(xe, $"Error initialising QParaDb");
                }
            }
        }
        private async Task<bool> MigrateAsync<T>(T db) where T : DbContext
        {
            var migrations = await db.Database.GetPendingMigrationsAsync();
            var result = migrations.Any();
            if (result)
            {
                await db.Database.MigrateAsync();
                log.Information($"{db.GetType().Name} schema migrated");
                foreach (var migration in migrations)
                {
                    log.Information($"\t{migration} applied");
                }

            }
            return result;
        }

    }
}
