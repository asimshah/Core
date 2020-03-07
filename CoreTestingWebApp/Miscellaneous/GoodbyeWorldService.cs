using Fastnet.Core;
using Fastnet.Core.Web;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTestingWebApp
{
    public class GoodbyeWorldService : HostedService
    {
        public static int count = 0;
        public GoodbyeWorldService(ILogger<GoodbyeWorldService> logger) : base(logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                log.Information($"Goodbye World from instance [{count}]");
                await Task.Delay(2000, stoppingToken);
            }
        }
        public void Echo(string message)
        {
            log.Information(message);
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //Debugger.Break();
            return base.StartAsync(cancellationToken);
        }
        protected override void OnStopped()
        {
            log.Information($"Goodbye World shutting down");
            base.OnStopped();
        }
    }
}
