using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Core
{
    /// <summary>
    /// Use this class to send and receive messages derived from MessageBase
    /// </summary>
    /// <remarks>Use as a singleton - typically via dependency injection</remarks>
    public class Messenger
    {
        private static MulticastSender mcastSender;
        private static MulticastListener mcastListener;
        private static SocketListener listener;
        private MessengerOptions options;
        private ILogger log;
        private ILoggerFactory loggerFactory;
        private readonly MessageRouter router;
        /// <summary>
        /// 
        /// </summary>
        public bool MulticastEnabled => mcastSender != null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="loggerFactory"></param>
        public Messenger(ILogger<Messenger> logger, IOptions<MessengerOptions> options, ILoggerFactory loggerFactory)
        {
            this.log = logger;
            this.options = options.Value;
            this.loggerFactory = loggerFactory;
            this.router = new MessageRouter(loggerFactory.CreateLogger<MessageRouter>());
            if (!this.options.DisableMulticastListening)
            {
                this.StartMulticastListener().ConfigureAwait(false);
                this.AddMulticastSubscription<MulticastTest>((m) => MulticastTestHandler(m as MulticastTest));
            }
        }
        /// <summary>
        /// Start a (tcp point-to-point) listener on the given address and port. Only one instance of a listener is supported
        /// </summary>
        /// <param name="address">an address such as 127.0.0.1</param>
        /// <param name="port">some suitable port no, such as 5858</param>
        /// <param name="onReceive">The method to call for each MessageBase derived message that arrives</param>
        /// <returns></returns>
        public async Task StartListener(string address, int port, Action<MessageBase> onReceive)
        {
            //Messenger.options = options;
            IPAddress ip;
            if (IPAddress.TryParse(address, out ip))
            {
                await StartListener(ip, port, onReceive);
            }
        }
        /// <summary>
        /// Stop the current listener
        /// </summary>
        public void StopListener()
        {
            listener?.Stop();
            listener = null;
        }
        /// <summary>
        /// Connect to a server listening on the given address and port. Use this at the client end
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns>The TcpClient that can be used for the SendAsync and Receive Async methods</returns>
        public TcpClient Connect(string address, int port)
        {
            //Messenger.options = options;
            var client = new TcpClient(address, port);
            client.NoDelay = true;
            return client;
        }
        /// <summary>
        /// Register a method to receive MessageBase derived messages on the give <see cref="System.Net.Sockets.TcpClient"/>
        /// </summary>
        /// <remarks>
        /// This is a method intended for use in a client to receive responses from the server.
        /// </remarks>
        /// <param name="client">A client obtained by calling <see cref="Connect"/></param>
        /// <param name="token">A cancellation token (required)</param>
        /// <param name="onMessageReceive">The method to call for each message received</param>
        /// <returns></returns>
        public async Task ReceiveAsync(TcpClient client, CancellationToken token, Action<MessageBase> onMessageReceive)
        {
            using (client)
            {
                var buffer = new byte[this.options.MaxMessageSize];
                var stream = client.GetStream(); ;
                PacketProtocol pp = new PacketProtocol(this.options);
                pp.MessageArrived = (data) =>
                {
                    var message = MessageBase.ToMessage(data, data.Length);
                    onMessageReceive?.Invoke(message);
                };
                try
                {
                    await pp.StartDataRead(stream, token);
                }
                catch (Exception xe)
                {
                    log.LogError(xe.Message);
                    throw;
                }

            }
        }
        /// <summary>
        /// Send a message to a server
        /// </summary>
        /// <param name="client">A client obtained by calling <see cref="Connect"/></param>
        /// <param name="message">The message (derived from MessageBase) to send</param>
        /// <returns></returns>
        public async Task SendAsync(TcpClient client, MessageBase message)
        {
            var stm = client.GetStream();
            await SendAsync(stm, message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalMessage"></param>
        /// <param name="reply"></param>
        /// <returns></returns>
        public async Task ReplyAsync(MessageBase originalMessage, MessageBase reply = null)
        {
            TcpClient replyTo = originalMessage.receivedFrom;
            if (replyTo == null)
            {
                throw new Exception("Cannot reply. Probable reason is that this message was not received from an Fastnet message listener");
            }
            if (reply == null)
            {
                reply = originalMessage;
            }
            await SendAsync(replyTo, reply);
        }
        /// <summary>
        /// Start a multi-cast listener
        /// </summary>
        /// <returns></returns>
        internal async Task StartMulticastListener()
        {
            if (mcastListener == null)
            {
                //this.router.AddDefaultMulticastRoute(defaultHandler);
                mcastListener = new MulticastListener(options, loggerFactory);
                await mcastListener.StartAsync((m) =>
                {
                    log.Trace($"Received {m.GetType().Name} with {m.DateTimeUtc.ToDefaultWithTime()}");
                    this.router.Route(m);
                });
            }
            else
            {
                log.Warning("Multicast Listener already started - only one instance is allowed, request ignored");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void AddMulticastSubscription<T>(Action<T> handler) where T : MessageBase
        {
            //this.router.AddMulticastSubscription<T>(handler as Action<MessageBase>);
            this.router.AddMulticastSubscription<T>(handler);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DiscardMessage<T>() where T : MessageBase
        {
            this.router.DiscardMessage<T>();
        }
        /// <summary>
        /// 
        /// </summary>
        public void StopMulticastListener()
        {
            mcastListener?.Stop();
            mcastListener = null;
        }
        /// <summary>
        /// Must be called before sending any multicast messages
        /// <para>Can only be called once per process</para>
        /// </summary>
        public void EnableMulticastSend()
        {
            if (mcastSender == null)
            {
                mcastSender = new MulticastSender(options, loggerFactory);
            }
            else
            {
                log.Debug("Multicast send already enabled, request ignored");
            }
        }
        /// <summary>
        /// Disables sending of multicast sends and cleans up internally
        /// </summary>
        public void DisableMulticastSend(/*IPAddress address, int port*/)
        {
            mcastSender?.Dispose();
            mcastSender = null;
        }
        /// <summary>
        /// Sends a single multicast message
        /// <para>All messages must be derived from MessageBase</para>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMulticastAsync(MessageBase message)
        {
            await mcastSender?.SendAsync(message);
            log.Trace($"Sent {message.GetType().Name} with {message.DateTimeUtc.ToDefaultWithTime()}");
        }
        /// <summary>
        /// Multicasts a test message
        /// </summary>
        /// <summary>Test messages are handled internally and results are sent via logging (as Information)</summary>
        /// <remarks>Test messages are handled internally and results are sent via logging (as Information)</remarks>
        /// <remarks> Any Fastnet web site that activates Messenger will automatically handle test messages</remarks>
        /// <returns></returns>
        public async Task SendMulticastTestAsync()
        {
            var mt = new MulticastTest();
            mt.SetNumber();
            mt.SourceMachine = Environment.MachineName.ToLower();
            mt.SourcePID = System.Diagnostics.Process.GetCurrentProcess().Id;
            await SendMulticastAsync(mt);
            log.Information($"MulticastTest message {mt.Number} sent at {mt.DateTimeUtc.ToDefaultWithTime()}");
        }
        /// <summary>
        /// Discards any incoming Multicast Test messages
        /// </summary>
        public void DiscardMulticastTestMessage()
        {
            this.router.DiscardMessage<MulticastTest>();
        }
        /// <summary>
        /// Start a listener on the given ip address and port. Only one instance of a listener is supported
        /// </summary>
        /// <param name="address">an address such one provided by IpAddress.TryParse</param>
        /// <param name="port">some suitable port no, such as 5858</param>
        /// <param name="onReceive">The method to call for each MessageBase derived message that arrives</param>
        /// <returns></returns>
        private async Task StartListener(IPAddress address, int port, Action<MessageBase> onReceive)
        {
            if (listener == null)
            {
                listener = new SocketListener(options, address, port);
                await listener.StartAsync(onReceive);
            }
            else
            {
                log.Information("Listener already started - only one instance is allowed, request ignored");

            }
        }
        /// <summary>
        /// NB: this method is not thread-safe!!!
        /// a future enhancement 
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendAsync(NetworkStream ns, MessageBase message)
        {
            try
            {
                var data = message.ToBytes();
                var wrappedData = PacketProtocol.WrapMessage(data);
                int index = 0;
                while (index < wrappedData.Length)
                {
                    int packetSize = (wrappedData.Length - index) > options.TransportBufferSize ? options.TransportBufferSize : (wrappedData.Length - index);
                    await ns.WriteAsync(wrappedData, index, packetSize);
                    index += packetSize;
                }
                log.Trace($"Sent: {message.GetType().Name} ({data.Length} bytes)");
            }
            catch (Exception xe)
            {
                log.LogError(xe.Message);
                //Debugger.Break();
                throw;
            }
        }
        private void MulticastTestHandler(MulticastTest m)
        {
            m.ReceivedAtUtc = DateTimeOffset.UtcNow;
            var elapsed = m.ReceivedAtUtc - m.DateTimeUtc;
            var message = $"MulticastTest message {m.Number} from {m.SourceMachine}, pid {m.SourcePID} received at {m.ReceivedAtUtc.ToDefaultWithTime()} in {elapsed.TotalMilliseconds}ms";
            if (elapsed.TotalMilliseconds > 250)
            {
                log.Warning($"{message} !!!!!!!!");
            }
            else
            {
                log.Information(message);
            }
        }
    }
}
