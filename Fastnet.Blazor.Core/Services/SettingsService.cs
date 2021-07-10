using Fastnet.Core;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// provides access to strongly typed items in appsettings.json. Note that the
    /// section name in appsettings has to be the simple type name (as obtained by <c>typeof(T).Name</c>)
    /// </summary>
    public class SettingsService : WebApiService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public SettingsService(HttpClient client, ILogger<SettingsService> logger) : base(client, logger)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetSetting<T>()
        {
            string query = $"settings/get/type/{typeof(T).Name}";
            var r = await GetAsync<T>(query);
            if(!r.Success)
            {
                var errors = string.Join(", ", r.Errors.Values.SelectMany(x => x));
                log.Error($"Failed to get setting of type {typeof(T).Name}: {errors}");
            }
            return r.Data;
        }
    }
}
