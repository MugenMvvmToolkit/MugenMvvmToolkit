#region Copyright

// ****************************************************************************
// <copyright file="EventAggregator.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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

using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public class EventAggregator : IEventAggregator, IHandler<object>
    {
        #region Fields

        private readonly List<ISubscriber> _subscribers;
        private readonly bool _trace;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventAggregator" /> class.
        /// </summary>
        public EventAggregator()
            : this(false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventAggregator" /> class.
        /// </summary>
        public EventAggregator(bool trace)
        {
            _trace = trace;
            _subscribers = new List<ISubscriber>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the list of subscribers.
        /// </summary>
        protected List<ISubscriber> Subscribers
        {
            get { return _subscribers; }
        }

        #endregion

        #region Implementation of IEventAggregator

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        void IHandler<object>.Handle(object sender, object message)
        {
            Publish(sender, message);
        }

        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">The message instance.</param>
        public virtual void Publish(object sender, object message)
        {
            Should.NotBeNull(sender, "sender");
            Should.NotBeNull(message, "message");
            if (_subscribers.Count == 0)
                return;
            ISubscriber[] subscribers;
            int size = 0;
            lock (_subscribers)
            {
                subscribers = new ISubscriber[_subscribers.Count];
                for (int i = 0; i < _subscribers.Count; i++)
                {
                    ISubscriber subscriber = _subscribers[i];
                    if (subscriber.IsAlive)
                        subscribers[size++] = subscriber;
                    else
                    {
                        _subscribers.RemoveAt(i);
                        --i;
                    }
                }
            }
            bool trace = _trace || message is ITracebleMessage;
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i].Handle(sender, message) == HandlerResult.Handled && trace)
                    Tracer.Warn("The message '{0}' from sender '{1}' was sended to '{2}'", message.GetType(),
                        sender.GetType(), subscribers[i].Target);
            }
        }

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="subscriber">The instance to subscribe for event publication.</param>
        public virtual bool Subscribe(ISubscriber subscriber)
        {
            Should.NotBeNull(subscriber, "subscriber");
            lock (_subscribers)
            {
                if (subscriber.AllowDuplicate || !Contains(subscriber, false))
                    _subscribers.Add(subscriber);
                return true;
            }
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="subscriber">The instance to unsubscribe.</param>
        public virtual bool Unsubscribe(ISubscriber subscriber)
        {
            Should.NotBeNull(subscriber, "subscriber");
            lock (_subscribers)
                return Contains(subscriber, true);
        }

        /// <summary>
        ///     Determines whether the <see cref="IEventAggregator" /> contains a specific subscriber.
        /// </summary>
        public virtual bool Contains(ISubscriber subscriber)
        {
            Should.NotBeNull(subscriber, "subscriber");
            lock (_subscribers)
                return Contains(subscriber, false);
        }

        /// <summary>
        ///     Removes all subscribers.
        /// </summary>
        public virtual void UnsubscribeAll()
        {
            lock (_subscribers)
                _subscribers.Clear();
        }

        /// <summary>
        ///     Gets the collection of subscribers.
        /// </summary>
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

        #endregion        
    }
}