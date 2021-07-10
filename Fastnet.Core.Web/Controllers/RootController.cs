using Fastnet.Core.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Fastnet.Core.Web
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            return new T[] { item };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        public static bool IsSuccess(this IActionResult actionResult)
        {
            if(actionResult is OkResult || actionResult is OkObjectResult)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static T Get<T>(this IActionResult result) where T : class
        {
            if(result is OkObjectResult)
            {
                return (result as OkObjectResult).Value as T;
            }
            throw new Exception($"{nameof(result)} is not OKObjectResult");
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public abstract class RootController : ControllerBase
    {
        private ILogger _log;
        /// <summary>
        /// 
        /// </summary>
        protected ILogger log => _log ?? (_log = GetLogger());

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="logger"></param>
        //public RootController(ILogger logger)
        //{
        //    log = logger;
        //}
        private ILogger GetLogger()
        {
            return HttpContext?.RequestServices.GetService<ILoggerFactory>().CreateLogger(this.GetType().Name);
        }
    }
}
