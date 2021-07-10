//using Microsoft.AspNet.Identity.EntityFramework;
//using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Fastnet.Core.UserAccounts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Fastnet.Core.Shared;

namespace Fastnet.Core.Web
{
    /// <summary>
    /// 
    /// </summary>
    public static class _userAccountExtensions
    {
        /// <summary>
        /// Enable Fastnet simple authentication using <cref>Fastnet.Core.UserAccounts.UserAccountDB</cref> and JWT tokens
        /// </summary>
        /// <remarks>
        /// Fastnet simple authentication requires an EFCore database in Sql Server that contains the UserAccountDb schema. It also
        /// requires an appsettings section called "JwtSettings" to configure JWT.
        /// This method adds the UserAccountDb DbContext, sets up Authentication using JWT and adds the UserManager as a scoped service
        /// </remarks>
        /// <example>
        /// JWTSettings in appsettings.json:
        /// <code>
        ///     "JWTSettings": {
        ///         "securityKey": "QParaQParaSecretKey",
        ///         "validIssuer": "QParaAPI",
        ///         "validAudience": "https://qpara.webframe.co.uk/",
        ///         "expiryInMinutes": 240
        ///     }
        /// </code>
        /// in the aspnetcore Startup clas, ConfigureServices() methods, call AddSimpleAuthentication as follows:
        /// <code>
        ///     var cs = environment.LocaliseConnectionString(Configuration.GetConnectionString("asimshahwebConnection")); // use this call to optionally 'localise' the connections string
        ///     services.AddSimpleAuthentication(Configuration, cs);
        /// </code>
        /// </example>
        /// <seealso cref="AccountsControllerBase"/>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IServiceCollection AddSimpleAuthentication(this IServiceCollection services, IConfiguration config,  string connectionString)
        {
            services.AddDbContext<UserAccountDb>(options =>
                {
                    options.UseSqlServer(connectionString);
                });
             services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var jwtSettings = config.GetSection("JwtSettings");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.GetSection("validIssuer").Value,
                        ValidAudience = jwtSettings.GetSection("validAudience").Value,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetSection("securityKey").Value)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            services.AddScoped<UserManager>();
            return services;
        }
    }
    /// <summary>
    ///  a base AccountsController to work with Fastnet.Core.UserAccounts.
    ///  Derive your own AccountsController from this base - you can then add further features to this controller.
    /// </summary>
    public abstract class AccountsControllerBase : RootController
    {
        private UserManager _userManager;
        internal UserManager userManager => _userManager ?? (_userManager = GetUserManager());
        /// <summary>
        /// 
        /// </summary>
        private UserManager GetUserManager()
        {
            return HttpContext?.RequestServices.GetService<UserManager>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        protected async Task<IActionResult> RegisterNewUserAsync(RegistrationModel newUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var checkForName = await userManager.FindUserAsync(newUser.Email);

            if (checkForName != null)
            {
                ModelState.AddModelError("Email", "This email is already in use");
                return BadRequest(ModelState);
            }
            //var user = new ApplicationUser { UserName = newUser.Email, Email = newUser.Email };
            var user = await userManager.CreateUserAsync(newUser.Email, newUser.Password);
            return Ok(user.ToDTO());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected async Task<string> RefreshCurrentUserTokenAsync()
        {
            var user = await userManager.FindUserAsync(User.Identity.Name);
            var token = userManager.GetToken(user);
            return token;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns>BadRequest or OKObjectResult with the token</returns>
        protected async Task<IActionResult> LoginUserAsync(LoginModel userModel)
        {
            var user = await userManager.FindUserAsync(userModel.Email);
            if (user == null || userManager.Verify(user, userModel.Password) == false)
            {
                ModelState.AddModelError(string.Empty, "Email and/or password invalid");
                return BadRequest(ModelState);
            }
            var token = userManager.GetToken(user);
            return Ok(token);
        }
    }
}
