using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;

namespace Fastnet.Core
{
    public static partial class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        public static void Clear(this DirectoryInfo dir/*, ILogger log = null*/)
        {
            if (dir.Exists)
            {
                foreach (var file in dir.EnumerateFiles())
                {
                    try
                    {
                        file.Delete();
                        log.Trace($"{file.FullName} deleted");
                    }
                    catch(FileNotFoundException)
                    {

                    }
                    catch (Exception)
                    {
                        log.Error($"failed to delete {file.FullName}");
                        throw;
                    }
                }
                foreach (var d in dir.EnumerateDirectories())
                {
                    int retryCount = 10;
                    while (retryCount > 0)
                    {
                        try
                        {
                            d.Clear();
                            d.Delete();
                            log.Trace($"{d.FullName} deleted");
                            break;
                        }
                        catch (Exception)
                        {
                            log.Error($"failed to delete {d.FullName}, retry count is {retryCount}");
                            Thread.Sleep(50);
                            --retryCount;
                        }
                    }
                } 
            }
            else
            {
                log.Warning($"directory {dir.FullName} unexpectedly not found");
            }
        }
    }

}
