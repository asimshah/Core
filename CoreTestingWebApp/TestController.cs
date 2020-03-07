using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoreTestingWebApp
{
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        public TestController(IServiceProvider sp, HelloWorldService hws)
        {
            this.serviceProvider = sp;
            var all = this.serviceProvider.GetServices<IHostedService>();
            Debugger.Break();
        }
        [HttpGet("get")]
        public string Get()
        {
            return "test get";
        }
    }
}