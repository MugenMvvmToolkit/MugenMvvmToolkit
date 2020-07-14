using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessengerHandlerSubscriber : HashSet<MessengerHandlerSubscriber.HandlerSubscriber>, IMessengerSubscriberComponent, IHasPriority
    {
        #region Fields

        private readonly IReflectionManager? _reflectionManager;

        private static readonly Dictionary<KeyValuePair<Type, Type>, Action<object?, object?, IMessageContext>?> Cache =
            new Dictionary<KeyValuePair<Type, Type>, Action<object?, object?, IMessageContext>?>(59, InternalComparer.TypeType);
        private static readonly Func<object, IMessageContext, object?, MessengerResult> HandlerDelegate = Handle;
        private static readonly Func<object, IMessageContext, object?, MessengerResult> HandlerRawDelegate = HandleRaw;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MessengerHandlerSubscriber(IReflectionManager? reflectionManager = null)
            : base(HandlerSubscriberEqualityComparer.Instance)
        {
            _reflectionManager = reflectionManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MessengerComponentPriority.Subscriber;

        #endregion

        #region Implementation of interfaces

        public bool TrySubscribe<TSubscriber>(IMessenger messenger, in TSubscriber subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TSubscriber>())
                return false;
            if (subscriber is IWeakReference weakReference && weakReference.Target is IMessengerHandler handler)
                return Add(new HandlerSubscriber(weakReference, RuntimeHelpers.GetHashCode(handler), executionMode));
            if (subscriber is IMessengerHandler)
                return Add(new HandlerSubscriber(subscriber, RuntimeHelpers.GetHashCode(subscriber), executionMode));
            return false;
        }

        public bool TryUnsubscribe<TSubscriber>(IMessenger messenger, in TSubscriber subscriber, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TSubscriber) == typeof(HandlerSubscriber))
                return Remove(MugenExtensions.CastGeneric<TSubscriber, HandlerSubscriber>(subscriber));
            if (TypeChecker.IsValueType<TSubscriber>())
                return false;
            if (subscriber is IWeakReference weakReference)
            {
                var target = weakReference.Target;
                if (target == null)
                    return Remove(weakReference);
                return Remove(new HandlerSubscriber(target, RuntimeHelpers.GetHashCode(target), null));
            }

            if (subscriber is IMessengerHandler)
                return Remove(new HandlerSubscriber(subscriber, RuntimeHelpers.GetHashCode(subscriber), null));

            return false;
        }

        public bool TryUnsubscribeAll(IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            bool unsubscribed;
            lock (this)
            {
                unsubscribed = Count != 0;
                Clear();
            }

            return unsubscribed;
        }

        public ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>> TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata)
        {
            var result = ItemOrListEditor.Get<MessengerHandler>(handler => handler.IsEmpty);
            var toRemove = ItemOrListEditor.Get<HandlerSubscriber>(subscriber => subscriber.Subscriber == null);
            lock (this)
            {
                if (Count == 0)
                    return default;

                foreach (var handler in this)
                {
                    var subscriber = handler.GetSubscriber();
                    if (subscriber == null)
                    {
                        toRemove.Add(handler);
                        continue;
                    }

                    var action = GetHandler(_reflectionManager, subscriber.GetType(), messageType);
                    if (action != null)
                        result.Add(new MessengerHandler(HandlerDelegate, handler.Subscriber, handler.ExecutionMode, action));
                    if (subscriber is IMessengerHandlerRaw handlerRaw && handlerRaw.CanHandle(messageType))
                        result.Add(new MessengerHandler(HandlerRawDelegate, handler.Subscriber, handler.ExecutionMode));
                }
            }

            var count = toRemove.Count;
            for (var i = 0; i < count; i++)
                messenger.TryUnsubscribe(toRemove[i], metadata);

            return result.ToItemOrList<IReadOnlyList<MessengerHandler>>();
        }

        public ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            lock (this)
            {
                if (Count == 0)
                    return default;
                if (Count == 1)
                {
                    var subscriber = this.FirstOrDefault();
                    return new MessengerSubscriberInfo(subscriber.Subscriber, subscriber.ExecutionMode);
                }

                return ItemOrList.FromListToReadOnly(this.ToArray(subscriber => new MessengerSubscriberInfo(subscriber.Subscriber, subscriber.ExecutionMode)));
            }
        }

        #endregion

        #region Methods

        private static MessengerResult Handle(object subscriber, IMessageContext context, object? handler)
        {
            if (subscriber is IWeakReference weakReference)
                subscriber = weakReference.Target!;
            if (subscriber == null || handler == null)
                return MessengerResult.Invalid;

            ((Action<object?, object?, IMessageContext>)handler).Invoke(subscriber, context.Message, context);
            return MessengerResult.Handled;
        }

        private static MessengerResult HandleRaw(object subscriber, IMessageContext context, object? _)
        {
            if (subscriber is IWeakReference weakReference)
                subscriber = weakReference.Target!;
            if (subscriber == null)
                return MessengerResult.Invalid;
            return ((IMessengerHandlerRaw)subscriber).Handle(context);
        }

        private new bool Add(HandlerSubscriber subscriber)
        {
            lock (this)
            {
                return base.Add(subscriber);
            }
        }

        private new bool Remove(HandlerSubscriber subscriber)
        {
            lock (this)
            {
                return base.Remove(subscriber);
            }
        }

        private bool Remove(IWeakReference subscriber)
        {
            bool result;
            lock (this)
            {
                HandlerSubscriber handler = default;
                foreach (var item in this)
                {
                    if (ReferenceEquals(item.Subscriber, subscriber))
                    {
                        handler = item;
                        break;
                    }
                }

                result = Remove(handler);
            }

            return result;
        }

        private static Action<object?, object?, IMessageContext>? GetHandler(IReflectionManager? reflectionManager, Type handlerType, Type messageType)
        {
            var key = new KeyValuePair<Type, Type>(handlerType, messageType);
            lock (Cache)
            {
                if (!Cache.TryGetValue(key, out var action))
                {
                    var interfaces = key.Key.GetInterfaces();
                    for (var index = 0; index < interfaces.Length; index++)
                    {
                        var @interface = interfaces[index];
                        if (!@interface.IsGenericType || @interface.GetGenericTypeDefinition() != typeof(IMessengerHandler<>))
                            continue;
                        var typeMessage = @interface.GetGenericArguments()[0];
                        var method = @interface.GetMethod(nameof(IMessengerHandler<object>.Handle), BindingFlagsEx.InstancePublic);
                        if (method != null && typeMessage.IsAssignableFrom(key.Value))
                            action += method.GetMethodInvoker<Action<object?, object?, IMessageContext>>(reflectionManager);
                    }

                    Cache[key] = action;
                }

                return action;
            }
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public readonly struct HandlerSubscriber
        {
            #region Fields

            public readonly ThreadExecutionMode? ExecutionMode;
            public readonly int Hash;
            public readonly object Subscriber;

            #endregion

            #region Constructors

            public HandlerSubscriber(object subscriber, int hashCode, ThreadExecutionMode? executionMode)
            {
                Subscriber = subscriber;
                Hash = hashCode;
                ExecutionMode = executionMode;
            }

            #endregion

            #region Methods

            public object? GetSubscriber()
            {
                if (Subscriber is IWeakReference w)
                    return w.Target;
                return Subscriber;
            }

            #endregion
        }

        private sealed class HandlerSubscriberEqualityComparer : IEqualityComparer<HandlerSubscriber>
        {
            #region Fields

            public static readonly IEqualityComparer<HandlerSubscriber> Instance = new HandlerSubscriberEqualityComparer();

            #endregion

            #region Constructors

            private HandlerSubscriberEqualityComparer()
            {
            }

            #endregion

            #region Implementation of interfaces

            public bool Equals(HandlerSubscriber x, HandlerSubscriber y)
            {
                return ReferenceEquals(x.Subscriber, y.Subscriber) || ReferenceEquals(x.GetSubscriber(), y.GetSubscriber());
            }

            public int GetHashCode(HandlerSubscriber obj)
            {
                return obj.Hash;
            }

            #endregion
        }

        #endregion
    }
}