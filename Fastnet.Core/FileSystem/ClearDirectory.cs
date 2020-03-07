using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;

namespace Fastnet.Core
{
    public static partial class extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="log"></param>
        public static void Clear(this DirectoryInfo dir, ILogger log = null)
        {
            foreach (var file in dir.EnumerateFiles())
            {
                try
                {
                    file.Delete();
                    log?.Trace($"{file.FullName} deleted");
                }
                catch (Exception)
                {
                    log?.Error($"failed to delete {file.FullName}");
                    throw;
                }
            }
            foreach (var d in dir.EnumerateDirectories())
            {
                int retryCount = 1;
                while (retryCount > 0)
                {
                    d.Clear(log);
                    try
                    {
                        d.Delete();
                        log?.Trace($"{d.FullName} deleted");
                    }
                    catch (Exception)
                    {
                        log?.Error($"failed to delete {d.FullName}");
                        Thread.Sleep(2000);
                        --retryCount;
                    }
                }
            }
        }
    }

}
