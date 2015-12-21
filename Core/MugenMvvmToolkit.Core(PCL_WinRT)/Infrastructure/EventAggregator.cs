#region Copyright

// ****************************************************************************
// <copyright file="EventAggregator.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class EventAggregator : IEventAggregator, IHandler<object>
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private struct MessageSenderCache
        {
            #region Fields

            public int Hash;
            public object Message;
            public object Sender;
            public ISubscriber Subscriber;

            #endregion

            #region Constructors

            public MessageSenderCache(object sender, object message, ISubscriber subscriber, int partHash)
            {
                Sender = sender;
                Message = message;
                Subscriber = subscriber;
                Hash = partHash;
            }

            #endregion
        }

        private sealed class MessageSenderCacheComparer : IEqualityComparer<MessageSenderCache>
        {
            #region Fields

            public static readonly MessageSenderCacheComparer Instance;

            #endregion

            #region Constructors

            static MessageSenderCacheComparer()
            {
                Instance = new MessageSenderCacheComparer();
            }

            private MessageSenderCacheComparer()
            {
            }

            #endregion

            #region Implementation of IEqualityComparer<in MessageSenderCache>

            public bool Equals(MessageSenderCache x, MessageSenderCache y)
            {
                return ReferenceEquals(x.Sender, y.Sender) && ReferenceEquals(x.Message, y.Message) &&
                       (ReferenceEquals(x.Subscriber, y.Subscriber) ||
                        (!x.Subscriber.AllowDuplicate && x.Subscriber.Equals(y.Subscriber)));
            }

            public int GetHashCode(MessageSenderCache obj)
            {
                unchecked
                {
                    return obj.Hash ^ obj.Subscriber.GetHashCode();
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly MessageSenderCache EmptyMessage;

        [ThreadStatic]
        private static HashSet<MessageSenderCache> _threadHandledMessages;

        private readonly List<ISubscriber> _subscribers;
        private readonly bool _trace;

        #endregion

        #region Constructors

        static EventAggregator()
        {
            var subscriber = new ActionSubscriber<object>((o, o1) => { });
            EmptyMessage = new MessageSenderCache(subscriber, subscriber, subscriber, 0);
        }

        public EventAggregator()
            : this(false)
        {
        }

        public EventAggregator(bool trace)
        {
            _trace = trace;
            _subscribers = new List<ISubscriber>();
        }

        #endregion

        #region Properties

        protected List<ISubscriber> Subscribers
        {
            get { return _subscribers; }
        }

        private static HashSet<MessageSenderCache> GetHandledMessages(out bool owner)
        {
            var handledMessages = _threadHandledMessages;
            if (handledMessages == null)
            {
                handledMessages = new HashSet<MessageSenderCache>(MessageSenderCacheComparer.Instance);
                _threadHandledMessages = handledMessages;
            }

            if (handledMessages.Count == 0)
            {
                owner = true;
                handledMessages.Add(EmptyMessage);
            }
            else
                owner = false;
            return handledMessages;
        }

        #endregion

        #region Implementation of IEventAggregator

        public virtual void Publish(object sender, object message)
        {
            Should.NotBeNull(sender, nameof(sender));
            Should.NotBeNull(message, nameof(message));
            if (_subscribers.Count == 0)
                return;
            bool owner;
            HashSet<MessageSenderCache> handledMessages = GetHandledMessages(out owner);
            int partHash = GetHash(sender, message);
            try
            {
                int size = 0;
                ISubscriber[] subscribers;
                lock (_subscribers)
                {
                    subscribers = new ISubscriber[_subscribers.Count];
                    for (int i = 0; i < _subscribers.Count; i++)
                    {
                        ISubscriber subscriber = _subscribers[i];
                        if (subscriber.IsAlive)
                        {
                            if (handledMessages.Add(new MessageSenderCache(sender, message, subscriber, partHash)))
                                subscribers[size++] = subscriber;
                        }
                        else
                        {
                            _subscribers.RemoveAt(i);
                            --i;
                        }
                    }
                }
                if (size != 0)
                {
                    bool trace = _trace || message is ITracebleMessage;
                    for (int i = 0; i < size; i++)
                    {
                        if (subscribers[i].Handle(sender, message) == HandlerResult.Handled && trace)
                            Trace(sender, message, subscribers[i].Target);
                    }
                }
            }
            finally
            {
                if (owner)
                    handledMessages.Clear();
            }
        }

        public virtual bool Subscribe(ISubscriber subscriber)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            lock (_subscribers)
            {
                if (subscriber.AllowDuplicate || !Contains(subscriber, false))
                    _subscribers.Add(subscriber);
                return true;
            }
        }

        public virtual bool Unsubscribe(ISubscriber subscriber)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            lock (_subscribers)
                return Contains(subscriber, true);
        }

        public virtual bool Contains(ISubscriber subscriber)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            lock (_subscribers)
                return Contains(subscriber, false);
        }

        public virtual void UnsubscribeAll()
        {
            lock (_subscribers)
                _subscribers.Clear();
        }

        public virtual IList<ISubscriber> GetSubscribers()
        {
            if (_subscribers.Count == 0)
                return Empty.Array<ISubscriber>();
            lock (_subscribers)
            {
                var subscribers = new List<ISubscriber>(_subscribers.Count);
                for (int i = 0; i < _subscribers.Count; i++)
                {
                    ISubscriber subscriber = _subscribers[i];
                    if (subscriber.IsAlive)
                        subscribers.Add(subscriber);
                    else
                    {
                        _subscribers.RemoveAt(i);
                        --i;
                    }
                }
                return subscribers;
            }
        }

        #endregion

        #region Methods

        public static void Publish(object target, object sender, object message, IDataContext context = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(sender, nameof(sender));
            Should.NotBeNull(message, nameof(message));
            Func<object, IDataContext, ISubscriber> converter = ServiceProvider.ObjectToSubscriberConverter;
            if (converter == null)
                return;
            ISubscriber subscriber = converter(target, context);
            bool owner;
            HashSet<MessageSenderCache> handledMessages = GetHandledMessages(out owner);
            try
            {
                if (handledMessages.Add(new MessageSenderCache(sender, message, subscriber, GetHash(sender, message))))
                {
                    if (subscriber.Handle(sender, message) == HandlerResult.Handled && message is ITracebleMessage)
                        Trace(sender, message, target);
                }
            }
            finally
            {
                if (owner)
                    handledMessages.Clear();
            }
        }

        private bool Contains(ISubscriber instance, bool remove)
        {
            for (int i = 0; i < _subscribers.Count; i++)
            {
                ISubscriber subscriber = _subscribers[i];
                if (subscriber.Equals(instance))
                {
                    if (remove)
                        _subscribers.RemoveAt(i);
                    return true;
                }
                if (!subscriber.IsAlive)
                {
                    _subscribers.RemoveAt(i);
                    --i;
                }
            }
            return false;
        }

        private static void Trace(object sender, object message, object target)
        {
            Tracer.Warn("The message '{0}' from '{1}' was sended to '{2}'", message.GetType(),
                sender.GetType(), target);
        }

        private static int GetHash(object sender, object message)
        {
            unchecked
            {
                return ((RuntimeHelpers.GetHashCode(sender) * 397) ^ RuntimeHelpers.GetHashCode(message) * 397);
            }
        }

        #endregion

        #region Implementation of IHandler<object>

        void IHandler<object>.Handle(object sender, object message)
        {
            Publish(sender, message);
        }

        #endregion
    }
}
