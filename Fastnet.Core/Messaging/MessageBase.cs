using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Fastnet.Core
{
    //
    [JsonObject]
    internal class MulticastTest : MessageBase
    {
        private static int index = 0;
        [JsonProperty]
        internal int Number { get; set; }
        [JsonProperty]
        internal DateTimeOffset ReceivedAtUtc { get; set; }
        [JsonProperty]
        internal string SourceMachine { get; set; }
        [JsonProperty]
        internal int SourcePID { get; set; }

        internal MulticastTest()
        {

        }
        internal void SetNumber()
        {
            this.Number = index++;
        }
        //

    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class MessageBase
    {
        private static JsonSerializerSettings jsonSettings;
        internal TcpClient receivedFrom;
        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset DateTimeUtc { get; set; }
        /// <summary>
        /// computer name of the machine from which the message was sent
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// process id of the process from which the message was sent
        /// </summary>
        public int ProcessId { get; set; }
        /// <summary>
        /// modulo 1024 sequence number
        /// </summary>
        public int SequenceNumber { get; set; }
        /// <summary>
        /// an arbitrary 'parcel' that the message will carry - use SetParcel() and GetParcel()
        /// </summary>
        public string JsonParcel { get; set; }
        /// <summary>
        /// Sets a 'parcel' for the message to carry
        /// </summary>
        /// <typeparam name="T">type of parcel</typeparam>
        /// <param name="obj">parcel instance</param>
        public void SetParcel<T>(T obj)
        {
            this.JsonParcel = obj.ToJson<T>();
        }
        /// <summary>
        /// gets the 'parcel' carried by the message
        /// </summary>
        /// <typeparam name="T">type of parcel</typeparam>
        /// <returns>parcel instance</returns>
        public T GetParcel<T>()

        {
            if (string.IsNullOrWhiteSpace(this.JsonParcel))
            {
                return default(T);
            }
            return this.JsonParcel.ToInstance<T>();
        }
        static MessageBase()
        {
            jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }
        /// <summary>
        /// 
        /// </summary>
        public MessageBase()
        {
            this.DateTimeUtc = DateTimeOffset.UtcNow;
            MachineName = Environment.MachineName.ToLower();
            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ToBytes()
        {
            //var json = JsonConvert.SerializeObject(this, jsonSettings);
            var json = this.ToJson(false, jsonSettings);
            return Encoding.UTF8.GetBytes(json);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static MessageBase ToMessage(byte[] data, int length) //where T: MessageBase
        {
            //Note: this works within a .net world because of TypeNameHandling.All
            // which adds some type information for JsonConvert
            // consequently I do not need generic version as in any case
            // it is not always the case that I know what type the message is at compile time
            var jsonString = Encoding.UTF8.GetString(data, 0, data.Length);
            var m = jsonString.ToInstance<MessageBase>(jsonSettings);
            return m as MessageBase;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
