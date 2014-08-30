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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private struct MessageSenderCache
        {
            #region Fields

            public readonly object Message;
            public readonly object Sender;

            #endregion

            #region Constructors

            public MessageSenderCache(object sender, object message)
            {
                Sender = sender;
                Message = message;
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
                return ReferenceEquals(x.Sender, y.Sender) && ReferenceEquals(x.Message, y.Message);
            }

            public int GetHashCode(MessageSenderCache obj)
            {
                unchecked
                {
                    return (RuntimeHelpers.GetHashCode(obj.Sender) * 397) ^ RuntimeHelpers.GetHashCode(obj.Message);
                }
            }

            #endregion
        }

        private sealed class HandlerInfo
        {
            #region Fields

            private const string HandlerInfoPath = "```@HandlerInfo";
            private static readonly HandlerInfo EmptyHandler;
            private static readonly Func<object, object, HandlerInfo> CreateDelegate;

            private static readonly Dictionary<Type, Dictionary<Type, Func<object, object[], object>>> TypeHandlers =
                new Dictionary<Type, Dictionary<Type, Func<object, object[], object>>>();

            private readonly Dictionary<Type, Func<object, object[], object>[]> _messageToHandlers;
            private readonly Dictionary<Type, Func<object, object[], object>> _handlers;
            private readonly HashSet<MessageSenderCache> _handledMessages;
            private readonly object _target;
            private readonly IHandler<object> _handler;
            private MessageSenderCache _lastMessage;

            private Type _lastType;
            private Func<object, object[], object>[] _lastValue;

            #endregion

            #region Constructors

            static HandlerInfo()
            {
                EmptyHandler = new HandlerInfo();
                CreateDelegate = Create;
            }

            private HandlerInfo()
            {
            }

            private HandlerInfo(object target, Dictionary<Type, Func<object, object[], object>> handlers)
            {
                _target = target;
                if (handlers.Count == 1 && target is ViewModelBase)
                    _handler = (IHandler<object>) target;
                else
                    _handlers = handlers;
                _messageToHandlers = new Dictionary<Type, Func<object, object[], object>[]>();
                _handledMessages = new HashSet<MessageSenderCache>(MessageSenderCacheComparer.Instance);
            }

            #endregion

            #region Properties

            public bool IsEmpty
            {
                get { return ReferenceEquals(this, EmptyHandler); }
            }

            public object Target
            {
                get { return _target; }
            }

            #endregion

            #region Methods

            public static HandlerInfo GetOrCreate(object target)
            {
                return ServiceProvider.AttachedValueProvider.GetOrAdd(target, HandlerInfoPath, CreateDelegate, null);
            }

            public void Handle(object sender, object message, bool trace)
            {
                if (ReferenceEquals(_target, sender))
                    return;
                var messageSenderCache = new MessageSenderCache(sender, message);
                lock (_handledMessages)
                {
                    if (MessageSenderCacheComparer.Instance.Equals(_lastMessage, messageSenderCache) ||
                        !_handledMessages.Add(messageSenderCache))
                        return;
                    _lastMessage = messageSenderCache;
                }
                try
                {
                    if (_handlers == null)
                    {
                        _handler.Handle(sender, message);
                        if (trace)
                            Trace(sender, message, _target);
                    }
                    else
                    {
                        var list = FilterHandlers(message.GetType());
                        for (int index = 0; index < list.Length; index++)
                        {
                            list[index].Invoke(_target, new[] { sender, message });
                            if (trace)
                                Trace(sender, message, _target);
                        }
                    }
                }
                finally
                {
                    lock (_handledMessages)
                    {
                        _handledMessages.Remove(messageSenderCache);
                        _lastMessage = default(MessageSenderCache);
                    }
                }
            }

            private static HandlerInfo Create(object target, object state)
            {
                var dictionary = GetHandlers(target);
                if (dictionary.Count == 0)
                    return EmptyHandler;
                return new HandlerInfo(target, dictionary);
            }

            private Func<object, object[], object>[] FilterHandlers(Type messageType)
            {
                lock (_messageToHandlers)
                {
                    if (_lastType == messageType)
                        return _lastValue;
                    Func<object, object[], object>[] value;
                    if (!_messageToHandlers.TryGetValue(messageType, out value))
                    {
                        var listValue = new List<Func<object, object[], object>>();
                        foreach (var pair in _handlers)
                        {
                            if (pair.Key.IsAssignableFrom(messageType))
                                listValue.Add(pair.Value);
                        }
                        value = listValue.ToArrayFast();
                        _messageToHandlers[messageType] = value;
                    }
                    _lastType = messageType;
                    _lastValue = value;
                    return value;
                }
            }

            private static Dictionary<Type, Func<object, object[], object>> GetHandlers(object handler)
            {
                lock (TypeHandlers)
                {
                    Type type = handler.GetType();
                    Dictionary<Type, Func<object, object[], object>> value;
                    if (!TypeHandlers.TryGetValue(type, out value))
                    {
                        value = new Dictionary<Type, Func<object, object[], object>>();
                        Type[] interfaces = type
                            .GetInterfaces()
#if PCL_WINRT
.Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition().Equals(typeof(IHandler<>)))
#else
.Where(x => x.IsGenericType && x.GetGenericTypeDefinition().Equals(typeof(IHandler<>)))
#endif
.ToArray();
                        for (int index = 0; index < interfaces.Length; index++)
                        {
                            Type @interface = interfaces[index];
                            Type typeMessage = @interface.GetGenericArguments()[0];
                            MethodInfo method = @interface.GetMethodEx("Handle");
                            value[typeMessage] = ServiceProvider.ReflectionManager.GetMethodDelegate(method);
                        }
                        TypeHandlers[type] = value;
                    }
                    return value;
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly List<WeakReference> _handlerReferences;
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
            _handlerReferences = new List<WeakReference>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the delegate that allows to notifies listener about an event.
        /// </summary>
        [CanBeNull]
        public static Action<object, object, object> PublishCustomAction { get; set; }

        #endregion

        #region Implementation of IObservable

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="instance">The instance to subscribe for event publication.</param>
        public virtual bool Subscribe(object instance)
        {
            Should.NotBeNull(instance, "instance");
            var handlerInfo = HandlerInfo.GetOrCreate(instance);
            if (handlerInfo.IsEmpty)
                return false;
            lock (_handlerReferences)
            {
                if (!Contains(instance, false))
                    _handlerReferences.Add(ServiceProvider.WeakReferenceFactory(handlerInfo, true));
            }
            return true;
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="instance">The instance to unsubscribe.</param>
        public virtual bool Unsubscribe(object instance)
        {
            Should.NotBeNull(instance, "instance");
            lock (_handlerReferences)
                return Contains(instance, true);
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
            if (_handlerReferences.Count == 0)
                return;
            bool trace = _trace || message is ITracebleMessage;
            HandlerInfo[] items;
            int size = 0;
            lock (_handlerReferences)
            {
                items = new HandlerInfo[_handlerReferences.Count];
                for (int index = 0; index < _handlerReferences.Count; index++)
                {
                    var handler = (HandlerInfo)_handlerReferences[index].Target;
                    if (handler == null)
                    {
                        _handlerReferences.RemoveAt(index);
                        index--;
                    }
                    else
                        items[size++] = handler;
                }
            }
            for (int index = 0; index < size; index++)
                items[index].Handle(sender, message, trace);
        }

        /// <summary>
        ///     Gets the collection of observers.
        /// </summary>
        public IList<object> GetObservers()
        {
            if (_handlerReferences.Count == 0)
                return Empty.Array<object>();
            var objects = new List<object>(_handlerReferences.Count);
            lock (_handlerReferences)
            {
                for (int index = 0; index < _handlerReferences.Count; index++)
                {
                    var handler = (HandlerInfo)_handlerReferences[index].Target;
                    if (handler == null)
                    {
                        _handlerReferences.RemoveAt(index);
                        index--;
                    }
                    else
                        objects.Add(handler.Target);
                }
            }
            return objects;
        }

        /// <summary>
        ///     Removes all listeners.
        /// </summary>
        public virtual void Clear()
        {
            lock (_handlerReferences)
                _handlerReferences.Clear();
        }

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        public virtual void Handle(object sender, object message)
        {
            Publish(sender, message);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Notifies listener about an event.
        /// </summary>
        /// <param name="target">The specified listener to notify.</param>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">The message instance.</param>
        public static void Publish([NotNull] object target, [NotNull] object sender, [NotNull] object message)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(sender, "sender");
            Should.NotBeNull(message, "message");
            var handlerInfo = HandlerInfo.GetOrCreate(target);
            if (!handlerInfo.IsEmpty)
                handlerInfo.Handle(sender, message, message is ITracebleMessage);
            var customAction = PublishCustomAction;
            if (customAction != null)
                customAction(target, sender, message);
        }

        private bool Contains(object instance, bool remove)
        {
            for (int index = 0; index < _handlerReferences.Count; index++)
            {
                var handler = (HandlerInfo)_handlerReferences[index].Target;
                if (handler == null)
                {
                    _handlerReferences.RemoveAt(index);
                    index--;
                }
                else if (ReferenceEquals(handler.Target, instance))
                {
                    if (remove)
                        _handlerReferences.RemoveAt(index);
                    return true;
                }
            }
            return false;
        }

        private static void Trace(object sender, object message, object target)
        {
            Tracer.Info("The message '{0}' from sender '{1}' was sended to '{2}'", message.GetType(), sender.GetType(),
                target);
        }

        #endregion
    }
}