using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Fastnet.Core.Web
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public static partial class Extensions
    {
        public static string UserAgent(this HttpRequest request)
        {
            return request.Headers["User-Agent"];
        }

        public static string GetComparableAddress(this IPAddress addr)
        {
            var text = addr.ToString();
            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                text = text.Substring(0, text.IndexOf("%"));
            }
            return text;
        }
        /// <summary>
        /// modifies a connection string containing |DataDirectory| to use the Data folder of the contentRoot
        /// and attaches "-dev" to the databasename if in a development environment
        /// </summary>
        /// <param name="env"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static string LocaliseConnectionString(this IHostEnvironment env, string cs)
        {
            var dataDirectory = Path.Combine(env.ContentRootPath, "Data");
            if (dataDirectory.CanAccess(true, true))
            {
                SqlConnectionStringBuilder cb = new SqlConnectionStringBuilder(cs);
                cb.AttachDBFilename = cb.AttachDBFilename.Replace("|DataDirectory|", Path.Combine(env.ContentRootPath, "Data"));
                if (env.IsDevelopment())
                {
                    cb.InitialCatalog = $"{cb.InitialCatalog}-dev";
                }
                return cb.ToString();
            }
            else
            {
                throw new Exception($"cannot access {dataDirectory}");
            }
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// 
    /// </summary>
    public static class VersionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetSiteVersion()
        {
            var assemblyLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            return System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
        }
        /// <summary>
        /// get versions for all the Fastnet assemblies in this image
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<AssemblyVersion> GetVersions()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetVersions();
        }
        /// <summary>
        /// logs all version information for the current site
        /// </summary>
        /// <param name="log"></param>
        /// <param name="siteName"></param>
        public static void Versions(this ILogger log, string siteName)
        {
            var name = Process.GetCurrentProcess().ProcessName;
            var siteVersion = GetSiteVersion();
            var versions = GetVersions();
            log.Information($"{siteName} {siteVersion} site started ({name}), using versions:");
            foreach (var item in versions.OrderByDescending(x => x.DateTime))
            {
                log.Information($"==> {item.Name}, {item.DateTime.ToDefaultWithTime()}, [{item.Version}, {item.PackageVersion}]");
            }
        }
    }
}
