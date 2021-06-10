using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fastnet.Core.Web
{
    public static partial class Extensions
    {
        /// <summary>
        /// Add a DBContext after modifying the connection string so that the db files
        /// are created in the web sites Data folder. Note that connections string in appsettings must
        /// specify AttachDbFilename and DataDirectory (as in AttachDbFilename=|DataDirectory|\\QParaDb.mdf, for example)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="environment"></param>
        /// <param name="connectionStringName">The name of the connection string in appsettings</param>
        /// <returns></returns>
        public static IServiceCollection AddLocalisedDbContext<T>(this IServiceCollection services,
            IConfiguration config, IWebHostEnvironment environment, string connectionStringName) where T : DbContext
        {
            var cs = environment.LocaliseConnectionString(config.GetConnectionString(connectionStringName));
            services.AddDbContext<T>(options =>
            {
                options.UseSqlServer(cs, sqlServerOptions =>
                {
                    if (environment.IsDevelopment())
                    {
                        sqlServerOptions.CommandTimeout(60 * 3);
                    }
                });
            });
            return services;
        }
    }
}
