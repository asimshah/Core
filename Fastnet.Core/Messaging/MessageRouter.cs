using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Fastnet.Core
{
    /// <summary>
    /// Routes MessageBase messages to handler by message type
    /// Normal use is as singleton
    /// </summary>
    internal class MessageRouter
    {
        private object sentinel = new object();
        private readonly List<Type> discardList;
        private readonly ILogger log;
        //private Dictionary<Type, List<Action<MessageBase>>> subscriptions;
        private Dictionary<Type, Delegate> subscriptions;
        /// <summary>
        /// 
        /// </summary>
        internal MessageRouter(ILogger log)
        {
            this.log = log;
            //subscriptions = new Dictionary<Type, List<Action<MessageBase>>>();
            subscriptions = new Dictionary<Type, Delegate>();
            discardList = new List<Type>();
        }
        //internal void AddDefaultMulticastRoute(Action<MessageBase> handler)
        //{
        //    this.defaultRoute = handler;
        //}
        /// <summary>
        /// Subscribe to a receive a message of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        internal void AddMulticastSubscription<T>(Action<T> handler) where T : MessageBase
        {
            lock (sentinel)
            {
                if (!subscriptions.ContainsKey((typeof(T))))
                {
                    //subscriptions.Add(typeof(T), new List<Action<MessageBase>>());
                    subscriptions.Add(typeof(T), handler);
                }
                else
                {
                    subscriptions[typeof(T)] = Delegate.Combine(subscriptions[typeof(T)], handler);
                }
            }
        }
        internal void DiscardMessage<T>() where T : MessageBase
        {
            Type t = typeof(T);
            if (!discardList.Contains(t))
            {
                discardList.Add(t);
            }
        }
        /// <summary>
        /// Call this on receipt of a message
        /// </summary>
        /// <param name="message"></param>
        internal void Route(MessageBase message)
        {
            Type t = message.GetType();
            if (!discardList.Contains(t))
            {
                //IEnumerable<Action<MessageBase>> handlerList = null;
                lock (sentinel)
                {
                    if (subscriptions.ContainsKey(t))
                    {
                        //handlerList = subscriptions[t];
                        subscriptions[t].DynamicInvoke(message);
                    }
                    else
                    {
                        do
                        {
                            t = t.BaseType;
                            log.Trace($"trying handler for {t.Name}");
                            if (subscriptions.ContainsKey(t))
                            {
                                subscriptions[t].DynamicInvoke(message);
                                break;
                            }

                        } while (t != typeof(MessageBase));
                    }
                }
            }
            else
            {
                log.Trace($"Message type {t.Name} discarded");
            }
        }
    }
}
