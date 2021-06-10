using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class SimpleAuthenticationStateProvider : AuthenticationStateProvider, IAuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly LocalStorage _localStorage;
        private readonly AuthenticationState _anonymous;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="localStorage"></param>
        public SimpleAuthenticationStateProvider(HttpClient httpClient, LocalStorage localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetAsync<string>("authToken");
            if (string.IsNullOrWhiteSpace(token))
                return _anonymous;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType")));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task NotifyUserAuthenticationAsync()
        {
            var token = await _localStorage.GetAsync<string>("authToken");
            if (string.IsNullOrWhiteSpace(token))
            {

                NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
            }
            else
            {
                var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType"));
                var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
                NotifyAuthenticationStateChanged(authState);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        public void NotifyUserAuthentication(string email)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, email) }, "jwtAuthType"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }
        /// <summary>
        /// 
        /// </summary>
        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
