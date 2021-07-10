using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class extensions
    {
        ///// <summary>
        ///// Adds required services for simple authentication. This includes LocalStorage, the SimpleAuthenticationStateProvider
        ///// the AuthenticationService and AddAuthorizationCore. Urls to the corresponding controller on the server must be provided.
        ///// </summary>
        ///// <param name="services"></param>
        ///// <param name="urls"></param>
        ///// <returns></returns>
        //public static IServiceCollection AddSimpleAuthentication(this IServiceCollection services, AuthenticationUrls urls)
        //{
        //    services.TryAddScoped<LocalStorage>();
        //    //services.AddScoped<LocalStorage>();
        //    services.AddScoped<AuthenticationStateProvider, SimpleAuthenticationStateProvider>();
        //    services.AddScoped<AuthenticationService>((sp) =>
        //    {
        //        var client = sp.GetRequiredService<HttpClient>();
        //        var localStorage = sp.GetRequiredService<LocalStorage>();
        //        var stateProvider = sp.GetRequiredService<AuthenticationStateProvider>();

        //        var logger = sp.GetService<ILogger<AuthenticationService>>();

        //        return new AuthenticationService(urls, logger, client, (IAuthenticationStateProvider)stateProvider, localStorage);
        //    });
        //    services.AddAuthorizationCore();
        //    return services;
        //}
        //public static IServiceCollection AddSimpleAuthentication<T>(this IServiceCollection services, /*AuthenticationUrls urls,*/
        //    Func<ILogger, HttpClient, IAuthenticationStateProvider, LocalStorage, T> implementationfactory ) where T : AuthenticationServiceBase
        public static IServiceCollection AddSimpleAuthentication<T>(this IServiceCollection services) where T : class, IAuthenticationService
        {
            services.TryAddScoped<LocalStorage>();
            services.AddScoped<AuthenticationStateProvider, SimpleAuthenticationStateProvider>();
            services.AddScoped<IAuthenticationService, T>();
            //services.AddScoped<AuthenticationServiceBase>((sp) =>
            //{
            //    var client = sp.GetRequiredService<HttpClient>();
            //    var localStorage = sp.GetRequiredService<LocalStorage>();
            //    var stateProvider = sp.GetRequiredService<AuthenticationStateProvider>();

            //    var logger = sp.GetService<ILogger<T>>();

            //    //return new AuthenticationService(urls, logger, client, (IAuthenticationStateProvider)stateProvider, localStorage);
            //    return implementationfactory(logger, client, (IAuthenticationStateProvider)stateProvider, localStorage);
            //});
            services.AddAuthorizationCore();
            return services;
        }
    }
}
