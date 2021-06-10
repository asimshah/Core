using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class extensions
    {
        /// <summary>
        /// Adds required services for simple authentication. This includes LocalStorage, the SimpleAuthenticationStateProvider
        /// the AuthenticationService and AddAuthorizationCore. Urls to the corresponding controller on the server must be provided.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="urls"></param>
        /// <returns></returns>
        public static IServiceCollection AddSimpleAuthentication(this IServiceCollection services, AuthenticationUrls urls)
        {
            services.TryAddScoped<LocalStorage>();
            //services.AddScoped<LocalStorage>();
            services.AddScoped<AuthenticationStateProvider, SimpleAuthenticationStateProvider>();
            services.AddScoped<AuthenticationService>((sp) =>
            {
                var client = sp.GetRequiredService<HttpClient>();
                var localStorage = sp.GetRequiredService<LocalStorage>();
                var stateProvider = sp.GetRequiredService<AuthenticationStateProvider>();

                var logger = sp.GetService<ILogger<AuthenticationService>>();
                //var urls = new AuthenticationUrls
                //{
                //    RegisterNewUser = "accounts/register",
                //    Login = "accounts/login",
                //    RefreshToken = "accounts/refresh"
                //};
                return new AuthenticationService(urls, logger, client, (IAuthenticationStateProvider)stateProvider, localStorage);
            });
            services.AddAuthorizationCore();
            return services;
        }
    }
}
