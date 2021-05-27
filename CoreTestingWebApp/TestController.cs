using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Fastnet.Core;
using Microsoft.Extensions.Logging;

namespace CoreTestingWebApp
{
    public class TestMulticast : MessageBase
    {

    }
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Messenger messenger;
        private readonly ILogger log;
        public TestController(ILogger<TestController> log, Messenger messenger, IServiceProvider sp/*, HelloWorldService hws*/)
        {
            this.log = log;
            this.serviceProvider = sp;
            this.messenger = messenger;
            if (!messenger.MulticastEnabled)
            {
                messenger.EnableMulticastSend();
            }
            //var all = this.serviceProvider.GetServices<IHostedService>();
            //Debugger.Break();
        }
        [HttpGet("get")]
        public string Get()
        {
            return "test get";
        }
        [HttpGet("testkey")]
        public void TestKey()
        {
            var result = a.b(@"11875bda055e8f4bcabdfc0b03712e78-us2", ApplicationKeys.QPara);
            var answer = a.a(result, ApplicationKeys.QPara);
            if (@"hello world" != answer)
            {
                Debugger.Break();
            }
            result = a.b(@"f81745b38c1d4c6ef1b8427bf387741c-us20", ApplicationKeys.QPara);
            answer = a.a(result, ApplicationKeys.QPara);
            if (@"all good men" != answer)
            {
                Debugger.Break();
            }
        }
        [HttpGet("testmc")]
        public async Task TestMc()
        {
            for (int i = 0; i < 10; ++i)
            {
                await messenger.SendMulticastAsync(new TestMulticast());
                log.Information($"multicast sent");
            }
        }

    }
}