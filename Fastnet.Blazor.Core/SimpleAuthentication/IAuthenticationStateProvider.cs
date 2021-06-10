using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAuthenticationStateProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<AuthenticationState> GetAuthenticationStateAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        void NotifyUserAuthentication(string email);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task NotifyUserAuthenticationAsync();
        /// <summary>
        /// 
        /// </summary>
        void NotifyUserLogout();
    }
}