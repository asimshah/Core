using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Fastnet.Core.Web
{
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
