using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
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
        private static readonly Dictionary<KeyValuePair<Type, Type>, Action<object?, object?, IMessageContext>?> Cache =
            new(59, InternalEqualityComparer.TypeType);

        private static readonly Func<object, IMessageContext, object?, MessengerResult> HandlerDelegate = Handle;
        private static readonly Func<object, IMessageContext, object?, MessengerResult> HandlerRawDelegate = HandleRaw;

        private readonly IReflectionManager? _reflectionManager;

        [Preserve(Conditional = true)]
        public MessengerHandlerSubscriber(IReflectionManager? reflectionManager = null)
        {
            _reflectionManager = reflectionManager;
        }

        public int Priority { get; init; } = MessengerComponentPriority.Subscriber;

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
            return ((IMessengerHandler)subscriber).Handle(context);
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

        public bool TrySubscribe(IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            if (subscriber is IWeakReference weakReference && weakReference.Target is IMessengerHandlerBase handler)
                return Add(new HandlerSubscriber(weakReference, RuntimeHelpers.GetHashCode(handler), executionMode));
            if (subscriber is IMessengerHandlerBase)
                return Add(new HandlerSubscriber(subscriber, RuntimeHelpers.GetHashCode(subscriber), executionMode));
            return false;
        }

        public bool TryUnsubscribe(IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata)
        {
            if (subscriber is IWeakReference weakReference)
            {
                var target = weakReference.Target;
                if (target == null)
                    return Remove(weakReference);
                return Remove(new HandlerSubscriber(target, RuntimeHelpers.GetHashCode(target), null));
            }

            if (subscriber is IMessengerHandlerBase)
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

        public bool HasSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata) => Count != 0;

        public ItemOrIReadOnlyList<MessengerHandler> TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata)
        {
            var result = new ItemOrListEditor<MessengerHandler>(2);
            var toRemove = new ItemOrListEditor<object>();
            lock (this)
            {
                if (Count == 0)
                    return default;

                foreach (var handler in this)
                {
                    var subscriber = handler.GetSubscriber();
                    if (subscriber == null)
                    {
                        toRemove.Add(handler.Subscriber);
                        continue;
                    }

                    var action = GetHandler(_reflectionManager, subscriber.GetType(), messageType);
                    if (action != null)
                        result.Add(new MessengerHandler(HandlerDelegate, handler.Subscriber, handler.ExecutionMode, action));
                    if (subscriber is IMessengerHandler handlerRaw && handlerRaw.CanHandle(messageType))
                        result.Add(new MessengerHandler(HandlerRawDelegate, handler.Subscriber, handler.ExecutionMode));
                }
            }

            var count = toRemove.Count;
            for (var i = 0; i < count; i++)
                messenger.TryUnsubscribe(toRemove[i], metadata);

            return result.ToItemOrList();
        }

        public ItemOrIReadOnlyList<MessengerSubscriberInfo> TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            lock (this)
            {
                if (Count == 0)
                    return default;

                var array = ItemOrArray.Get<MessengerSubscriberInfo>(Count);
                var index = 0;
                foreach (var item in this)
                    array.SetAt(index++, new MessengerSubscriberInfo(item.Subscriber, item.ExecutionMode));
                return array;
            }
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
            lock (this)
            {
                foreach (var item in this)
                {
                    if (item.Subscriber == subscriber)
                        return Remove(item);
                }
            }

            return false;
        }

        [StructLayout(LayoutKind.Auto)]
        public readonly struct HandlerSubscriber : IEquatable<HandlerSubscriber>
        {
            public readonly ThreadExecutionMode? ExecutionMode;
            public readonly int Hash;
            public readonly object Subscriber;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public HandlerSubscriber(object subscriber, int hashCode, ThreadExecutionMode? executionMode)
            {
                Subscriber = subscriber;
                Hash = hashCode;
                ExecutionMode = executionMode;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public object? GetSubscriber()
            {
                if (Subscriber is IWeakReference w)
                    return w.Target;
                return Subscriber;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(HandlerSubscriber other) => Subscriber == other.Subscriber || GetSubscriber() == other.GetSubscriber();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode() => Hash;
        }
    }
}