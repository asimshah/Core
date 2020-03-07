using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


namespace Fastnet.Core
{
    internal static class udpExtensions
    {
        public static void EnableMulticast(this UdpClient client, string multicastAddress, int multicastPort, string localAddress)
        {
            IPAddress multicastIpAddress = IPAddress.Parse(multicastAddress);
            IPAddress localIpAddress = null;
            IPNetwork localNetwork = null;
            if (IPNetwork.TryParse(localAddress, out localNetwork))
            {
                localIpAddress = GetLocalIpAddress(localNetwork);

            }
            else
            {
                localIpAddress = IPAddress.Parse(localAddress);
            }
            var localEndPoint = new IPEndPoint(localIpAddress, multicastPort);
            client.ExclusiveAddressUse = false;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Bind, Join
            client.Client.Bind(localEndPoint);
            client.JoinMulticastGroup(multicastIpAddress, localIpAddress);
            //return client;
        }

        private static IPAddress GetLocalIpAddress(IPNetwork localNetwork)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (localNetwork.Contains(ip))
                {
                    return ip;
                }
                //if (IPNetwork.Contains(localNetwork, ip))
                //{
                //    return ip;
                //}
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
    internal class MulticastListener
    {
        private CancellationTokenSource cts;
        private UdpClient client;
        private ILogger log;
        private readonly MessengerOptions options;
        private string localMachine = Environment.MachineName.ToLower();
        private int currentPid = System.Diagnostics.Process.GetCurrentProcess().Id;
        internal MulticastListener(MessengerOptions options, ILoggerFactory lf)
        {
            try
            {
                this.options = options;
                this.log = lf.CreateLogger<MulticastListener>();
                client = new UdpClient();
                client.EnableMulticast(options.MulticastIPAddress, options.MulticastPort, options.LocalCIDR);
            }
            catch (Exception xe)
            {
                log.Error(xe);
                throw;
            }
        }
        internal async Task StartAsync(Action<MessageBase> onMessageReceive)
        {
            cts = new CancellationTokenSource();
            //IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            log.Information($"listener started on {this.options.MulticastIPAddress}:{this.options.MulticastPort}");
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    using (cts.Token.Register(client.Dispose))
                    {
                        try
                        {
                            UdpReceiveResult result = await client.ReceiveAsync();
                            DispatchResult(result, onMessageReceive);
                        }
                        catch (AggregateException ae)
                        {
                            ae.Handle((e) =>
                            {
                                if (e is TaskCanceledException)
                                {
                                    log.Warning("TaskCanceledException occurred");
                                }
                                return e is TaskCanceledException;
                            });
                        }
                        catch (TaskCanceledException)
                        {
                            log.Debug("Task cancelled");
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    log.Debug("Task cancelled");
                }
                catch (Exception xe)
                {
                    log.Error(xe);
                    //throw;
                }
            }
        }
        internal void Stop()
        {
            cts.Cancel();
            log.Debug("listener stopped");
        }
        private void DispatchResult(UdpReceiveResult result, Action<MessageBase> onMessageReceive)
        {
            Task.Run(() =>
            {
                try
                {
                    var message = MessageBase.ToMessage(result.Buffer, result.Buffer.Length);

                    if (!(message.MachineName == localMachine && message.ProcessId == currentPid))
                    {
                        log.Trace($"received {message.ToJson()}, type {message.GetType().Name}, from {result.RemoteEndPoint.ToString()}");
                        onMessageReceive(message);
                    }
                }
                catch (Exception xe)
                {
                    log.Error(xe);
                }
            });
        }
    }
}
