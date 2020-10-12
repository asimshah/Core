using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;


namespace Fastnet.Core.Web
{
    /// <summary>
    /// To use: (1) services.AddScoped&lt;WebServiceCallTrace&gt;(); (2) [ServiceFilter(typeof(WebServiceCallTrace))] on controller
    /// </summary>
    public class WebServiceCallTrace : ActionFilterAttribute
    {
        private readonly ILogger log;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public WebServiceCallTrace(ILogger<WebServiceCallTrace> log)
        {
            this.log = log;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var path = context.HttpContext.Request.Path;
            var descr = context.ActionDescriptor as ControllerActionDescriptor;
            log.Information($"{path} ==> {context.Controller.GetType().Name} ==> {descr.ActionName}()");
            base.OnActionExecuting(context);
        }
    }
}
