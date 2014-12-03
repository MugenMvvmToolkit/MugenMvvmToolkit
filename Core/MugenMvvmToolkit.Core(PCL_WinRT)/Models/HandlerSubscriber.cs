#region Copyright
// ****************************************************************************
// <copyright file="HandlerSubscriber.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Models
{
    internal sealed class HandlerSubscriber : ISubscriber
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

        #endregion

        #region Fields

        private const string HandlerInfoPath = "```@HandlerInfo";
        private static readonly HandlerSubscriber EmptyHandler;
        private static readonly Func<object, object, HandlerSubscriber> CreateDelegate;
        private static readonly Dictionary<Type, Dictionary<Type, Func<object, object[], object>>> TypeHandlers;

        private readonly Dictionary<Type, Func<object, object[], object>[]> _messageToHandlers;
        private readonly Dictionary<Type, Func<object, object[], object>> _handlers;
        private readonly HashSet<MessageSenderCache> _handledMessages;
        private readonly int _hashCode;
        private readonly WeakReference _reference;

        private MessageSenderCache _lastMessage;
        private Type _lastType;
        private Func<object, object[], object>[] _lastValue;

        #endregion

        #region Constructors

        static HandlerSubscriber()
        {
            TypeHandlers = new Dictionary<Type, Dictionary<Type, Func<object, object[], object>>>();
            EmptyHandler = new HandlerSubscriber();
            CreateDelegate = Create;
        }

        private HandlerSubscriber()
        {
        }

        public HandlerSubscriber(object target, Dictionary<Type, Func<object, object[], object>> handlers)
        {
            _hashCode = RuntimeHelpers.GetHashCode(target);
            _reference = ToolkitExtensions.GetWeakReference(target);
            if (handlers.Count != 1 || !(target is ViewModelBase))
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

        #endregion

        #region Implementation of ISubscriber

        public bool IsAlive
        {
            get { return _reference.Target != null; }
        }

        public bool AllowDuplicate
        {
            get { return false; }
        }

        public object Target
        {
            get { return _reference.Target; }
        }

        public HandlerResult Handle(object sender, object message)
        {
            var target = _reference.Target;
            if (target == null)
                return HandlerResult.Invalid;
            var messageSenderCache = new MessageSenderCache(sender, message);
            lock (_handledMessages)
            {
                if (MessageSenderCacheComparer.Instance.Equals(_lastMessage, messageSenderCache) ||
                    !_handledMessages.Add(messageSenderCache))
                    return HandlerResult.Ignored;
                _lastMessage = messageSenderCache;
            }
            try
            {
                if (_handlers == null)
                    ((IHandler<object>)target).Handle(sender, message);
                else
                {
                    var list = FilterHandlers(message.GetType());
                    if (list.Length == 0)
                        return HandlerResult.Ignored;
                    for (int index = 0; index < list.Length; index++)
                        list[index].Invoke(target, new[] { sender, message });
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
            return HandlerResult.Handled;
        }

        #endregion

        #region Methods

        public static HandlerSubscriber GetOrCreate(object target)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(target, HandlerInfoPath, CreateDelegate, null);
        }

        private static HandlerSubscriber Create(object target, object state)
        {
            var dictionary = GetHandlers(target);
            if (dictionary.Count == 0)
                return EmptyHandler;
            return new HandlerSubscriber(target, dictionary);
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
                    value = listValue.ToArrayEx();
                    _messageToHandlers[messageType] = value;
                }
                _lastType = messageType;
                _lastValue = value;
                return value;
            }
        }

        #endregion

        #region Equality members

        public bool Equals(ISubscriber other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ReferenceEquals(Target, other.Target);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HandlerSubscriber)obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        #endregion
    }
}