using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;

namespace Fastnet.Core.UserAccounts
{
    public class UserAccountDbFactory : IDesignTimeDbContextFactory<UserAccountDb>
    {
        public UserAccountDb CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", true)
                 .AddEnvironmentVariables()
                 .Build();

            var builder = new DbContextOptionsBuilder<UserAccountDb>();

            var connectionString = configuration
                        .GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString,
                        x => x.MigrationsAssembly(typeof(UserAccountDbFactory).Assembly.FullName));



            return new UserAccountDb(builder.Options);
        }
    }
}
