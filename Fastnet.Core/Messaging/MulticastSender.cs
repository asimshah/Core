using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class AMAliveMulticast : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }
    }
    internal class MulticastSender : IDisposable
    {
        const int maxSequenceNumber = 1024;
        private int sequenceNumber;
        private UdpClient sendClient;
        private IPEndPoint sendTo;
        private BlockingCollection<MessageBase> messageQueue;
        private ILogger log;
        private readonly MessengerOptions options;
        internal MulticastSender(MessengerOptions options
            //string multicastAddress, int multicastPort, string localIpAddress
            , ILoggerFactory lf)
        {
            this.options = options;
            this.log = lf.CreateLogger<MulticastSender>();
            //sendClient = new UdpClient();
            //sendClient.EnableMulticast(options.MulticastIPAddress, options.MulticastPort, options.LocalCIDR);
            //sendTo = new IPEndPoint(IPAddress.Parse(options.MulticastIPAddress), options.MulticastPort);
            Task.Run(async () => await InitialiseQueue());
            log.Information($"sender enabled, {options.MulticastIPAddress}:{options.MulticastPort}");
        }

        private async Task InitialiseQueue()
        {
            while (true)
            {
                //await Task.Run(async () =>
                //{
                    try
                    {
                        if (sendClient == null)
                        {
                            sendClient = new UdpClient();
                            sendClient.EnableMulticast(options.MulticastIPAddress, options.MulticastPort, options.LocalCIDR);
                            sendTo = new IPEndPoint(IPAddress.Parse(options.MulticastIPAddress), options.MulticastPort);
                            messageQueue = new BlockingCollection<MessageBase>();
                            await ServiceQueue();

                            sendClient.Close();
                            sendClient.Dispose();
                            sendClient = null;
                            //await Task.Delay(5000);

                            log.Warning($"Restarting udp client");
                        }
                        //Debugger.Break();
                    }
                    catch (Exception xe)
                    {
                        log.Error(xe);
                    }
                //});
                await Task.Delay(5000);
            }
        }
        private async Task ServiceQueue()
        {
            int getNextSequence()
            {
                int next = Interlocked.Increment(ref sequenceNumber);
                return next % maxSequenceNumber;
            }
            while (true || !messageQueue.IsCompleted)
            {
                MessageBase message = null;
                try
                {
                    message = messageQueue.Take();
                }
                catch (InvalidOperationException) { }
                catch (Exception xe)
                {
                    log.Error(xe);
                }
                if (message != null)
                {
                    //try
                    //{
                        message.SequenceNumber = getNextSequence();
                        message.DateTimeUtc = DateTimeOffset.UtcNow;
                        var data = message.ToBytes();
                        await sendClient.SendAsync(data, data.Length, sendTo);
                        log.Information($"Sent {message.ToJson()}, type {message.GetType().Name} to {sendTo.ToString()}");
                    //}
                    //catch (Exception xe)
                    //{
                    //    log.Error(xe);
                    //}
                }
                else
                {
                    log.Information($"no message!");
                }
            }
        }
        public async Task SendAsync(MessageBase message)
        {
            await Task.Delay(0);
            try
            {
                messageQueue.Add(message);
                //var data = message.ToBytes();
                //await sendClient.SendAsync(data, data.Length, sendTo);
                //log.Trace($"Sent {message.ToJson()}, type {message.GetType().Name} to {sendTo.ToString()}");
            }
            catch (Exception xe)
            {
                log.Error(xe);
                //throw;
            }
        }
        public async Task SendAsyncOld(MessageBase message)
        {
            try
            {
                var data = message.ToBytes();
                await sendClient.SendAsync(data, data.Length, sendTo);
                log.Trace($"Sent {message.ToJson()}, type {message.GetType().Name} to {sendTo.ToString()}");
            }
            catch (Exception xe)
            {
                log.Error(xe);
                //throw;
            }
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (sendClient != null)
                    {
                        sendClient.Close();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
