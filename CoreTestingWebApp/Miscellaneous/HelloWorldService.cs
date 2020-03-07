using Fastnet.Core;
using Fastnet.Core.Web;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTestingWebApp
{
    public class HelloWorldService : HostedService // BackgroundService
    {
        public static int count = 0;
        public HelloWorldService(ILogger<HelloWorldService> logger/*, IHostApplicationLifetime lifetime*/) : base(logger )
        {
            count++;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                log.Information($"Hello World from instance [{count}]");
                await Task.Delay(2000, stoppingToken);
            }
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //Debugger.Break();
            return base.StartAsync(cancellationToken);
        }
        protected override void OnStopped()
        {
            log.Information($"Hello World shutting down");
            base.OnStopped();
        }
    }
}
