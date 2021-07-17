using Fastnet.Core;
using Fastnet.Core.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Fastnet.Blazor.Core
{

    public interface IAuthenticationService
    {
        Task<WebApiResult<string>> LoginAsync<T>(T userForlogin/*, ValidatorCollection validators = null*/) where T : LoginModel;
        Task LogoutAsync();
        Task<WebApiResult<string>> RefreshTokenAsync();
        Task<WebApiResult<UserAccountDTO>> RegisterUserAsync<T>(T userForRegistration/*, ValidatorCollection validators = null*/) where T : RegistrationModel;
        void SetOnUnAuthorised(Action method);
    }

    /// <summary>
    /// Base class for an authentication service to work with an AccountsController derived from AccountsControllerBase
    /// Use this base class to create an authentication service with custom controller urls
    /// </summary>
    public abstract class AuthenticationServiceBase : WebApiService, IAuthenticationService
    {
        private readonly IAuthenticationStateProvider _authStateProvider;
        private readonly LocalStorage _localStorage;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="client"></param>
        /// <param name="authStateProvider"></param>
        /// <param name="localStorage"></param>
        public AuthenticationServiceBase(ILogger logger, HttpClient client, AuthenticationStateProvider authStateProvider,
            LocalStorage localStorage) : base(client, logger)
        {
            _authStateProvider = authStateProvider as IAuthenticationStateProvider;
            _localStorage = localStorage;
        }
        public abstract Task<WebApiResult<UserAccountDTO>> RegisterUserAsync<T>(T userForRegistration) where T : RegistrationModel;
        public abstract Task<WebApiResult<string>> LoginAsync<T>(T userForLogin) where T : LoginModel;
        public abstract Task<WebApiResult<string>> RefreshTokenAsync();
        /// <summary>
        /// Sets a token for the given emal
        /// </summary>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        protected async Task<string> SetTokenAsync(string token, string email)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(token));
            //log.Information($"setting token {token}");
            await _localStorage.SetAsync<string>("authToken", token);
            _authStateProvider.NotifyUserAuthentication(email); // why this and the next???
            await _authStateProvider.NotifyUserAuthenticationAsync(); // why this and thew previous??
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            return token;
        }
        /// <summary>
        /// Clears the current token
        /// </summary>
        /// <returns></returns>
        public virtual async Task LogoutAsync()
        {
            await _localStorage.RemoveAsync("authToken");
            _authStateProvider.NotifyUserLogout();
            client.DefaultRequestHeaders.Authorization = null;
        }
        /// <summary>
        /// Call this to replace an existing token with a fresh one for the current user
        /// This has the effect of renewing the expiry time
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshTokenAsync(string token)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(token));
            //log.Information($"resetting token to {token}");
            await _localStorage.SetAsync<string>("authToken", token);
            await _authStateProvider.NotifyUserAuthenticationAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        }
        internal async Task ClearCurrentTokenAsync()
        {
            await _localStorage.RemoveAsync("authToken");
            client.DefaultRequestHeaders.Authorization = null;
            log.Warning($"token cleared internally");
        }
    }
}
