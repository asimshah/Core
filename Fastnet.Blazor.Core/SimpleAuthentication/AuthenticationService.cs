using Fastnet.Core.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// a set of urls to work with an AccountsController derived from AccountsControllerBase
    /// </summary>
    public class AuthenticationUrls
    {
        /// <summary>
        /// the url to register a new user, e.g. "accounts/register". This must be a POST method.
        /// </summary>
        public string RegisterNewUser { get; init; }
        /// <summary>
        /// the url to a login a user, e.g. "accounts/login". This must be a POST method.
        /// </summary>
        public string Login { get; init; }
        /// <summary>
        /// the url to refresh a user's bearer token, e.g. "accounts/refresh". This must be a GET method.
        /// </summary>
        public string RefreshToken { get; init; }
    }

    /// <summary>
    /// an authentication service to work with an AccountsController derived from AccountsControllerBase 
    /// </summary>
    public class AuthenticationService : WebApiService
    {
        private readonly IAuthenticationStateProvider _authStateProvider;
        private readonly LocalStorage _localStorage;
        private readonly AuthenticationUrls urls;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="logger"></param>
        /// <param name="client"></param>
        /// <param name="authStateProvider"></param>
        /// <param name="localStorage"></param>
        public AuthenticationService(AuthenticationUrls urls,  ILogger<AuthenticationService> logger, HttpClient client, IAuthenticationStateProvider authStateProvider,
            LocalStorage localStorage) : base(client, logger)
        {
            //http = client;
            this.urls = urls;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userForRegistration"></param>
        /// <returns></returns>
        public async Task<UserAccountDTO> RegisterUserAsync(RegistrationModel userForRegistration)
        {
            var rm = await PostAsync<RegistrationModel, UserAccountDTO>(urls.RegisterNewUser, userForRegistration);
            return rm;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userForAuthentication"></param>
        /// <returns></returns>
        public async Task<string> LoginAsync(LoginModel userForAuthentication)
        {
            var token = await PostAsync<LoginModel, string>(urls.Login, userForAuthentication);
            await _localStorage.SetAsync<string>("authToken", token);
            _authStateProvider.NotifyUserAuthentication(userForAuthentication.Email); // why this and the next???
            await _authStateProvider.NotifyUserAuthenticationAsync(); // why this and thew previous??
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            return token;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task LogoutAsync()
        {
            await _localStorage.RemoveAsync("authToken");
            _authStateProvider.NotifyUserLogout();
            client.DefaultRequestHeaders.Authorization = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task RefreshTokenAsync()
        {
            string token = await GetAsync<string>(urls.RefreshToken);
            await _localStorage.SetAsync<string>("authToken", token);
            await _authStateProvider.NotifyUserAuthenticationAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        }
    }
}
