using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Fastnet.Core;

namespace Fastnet.Core.UserAccounts
{
    /// <summary>
    /// 
    /// </summary>
    public class UserManager
    {
        private readonly UserAccountDb db;
        private readonly ILogger log;
        private readonly IConfigurationSection jwtSettings;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public UserManager(UserAccountDb db, IConfiguration configuration, ILogger<UserManager> logger)
        {
            this.db = db;
            this.log = logger;
            jwtSettings = configuration.GetSection("JwtSettings");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UserAccount> CreateUserAsync(string email, string password)
        {
            var user = await FindUserAsync(email);
            if(user == null)
            {
                var passwordhash = password.Hash();
                user = new UserAccount { Email = email, PasswordHash = passwordhash };
                await db.UserAccounts.AddAsync(user);
                await db.SaveChangesAsync();
                log.Information($"new user {email} created");
                return user;
            }
            else
            {
                throw new Exception($"user with email {email} already exists");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Verify(UserAccount user, string password)
        {
            return password.Verify(user.PasswordHash);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<UserAccount> FindUserAsync(string email)
        {
            email = email.ToLower();
            UserAccount user = await db.UserAccounts.SingleOrDefaultAsync(x => x.Email == email);
            return user;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task CreateRoleAsync(string roleName)
        {
            roleName = roleName.ToLower();
            if((await db.Roles.SingleOrDefaultAsync(x => x.Name == roleName)) == null)
            {
                await db.Roles.AddAsync(new Role { Name = roleName});
                await db.SaveChangesAsync();
                log.Information($"new role {roleName} added");
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task AddUserToRoleAsync(UserAccount user, Role role)
        {
            if(!user.Roles.Contains(role))
            {
                await db.UserRoles.AddAsync(new UserRole { Role = role, User = user });
                log.Information($"user {user.Email} added to {role.Name}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task AddUserToRoleAsync(UserAccount user, string roleName)
        {
            roleName = roleName.ToLower();
            Role role = await db.Roles.SingleOrDefaultAsync(x => x.Name == roleName);
            if (role != null)
            {
                await AddUserToRoleAsync(user, role);
            }
            else
            {
                throw new Exception($"role {roleName} not found");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GetToken(UserAccount user)
        {
            var signingCredentials = GetSigningCredentials();
            var roles = GetRoles(user);
            var claims = GetClaims(user, roles);
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetUserCount()
        {
            return db.UserAccounts.Count();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetRoleCount()
        {
            return db.Roles.Count();
        }
        private IEnumerable<string> GetRoles(UserAccount user)
        {
            return user.Roles.Select(x => x.Name);
        }
        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(jwtSettings.GetSection("securityKey").Value);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        private List<Claim> GetClaims(UserAccount user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var expiryInterval = Convert.ToDouble(jwtSettings.GetSection("expiryInMinutes").Value);
            var expiry = DateTime.Now.AddMinutes(expiryInterval);
            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings.GetSection("validIssuer").Value,
                audience: jwtSettings.GetSection("validAudience").Value,
                claims: claims,
                expires: expiry,
                signingCredentials: signingCredentials);
            return tokenOptions;
        }
    }
}
