using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class MugenExtensions
    {
        #region Methods

        #region BusyIndicatorProvider

        public static TTask WithBusyIndicator<TTask>(this TTask task, IViewModel viewModel, object? message = null, int millisecondsDelay = 0)
            where TTask : Task
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return task.WithBusyIndicator(viewModel.BusyIndicatorProvider, message, millisecondsDelay);
        }

        public static TTask WithBusyIndicator<TTask>(this TTask task, IBusyIndicatorProvider busyIndicatorProvider, object? message = null, int millisecondsDelay = 0)
            where TTask : Task
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(busyIndicatorProvider, nameof(busyIndicatorProvider));
            if (task.IsCompleted)
                return task;
            if (millisecondsDelay == 0 && message is IHasBusyDelayMessage hasBusyDelay)
                millisecondsDelay = hasBusyDelay.Delay;
            var token = busyIndicatorProvider.Begin(message, millisecondsDelay);
            task.ContinueWith((t, o) => ((IBusyToken)o).Dispose(), token, TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static void ClearBusy(this IBusyIndicatorProvider provider)
        {
            Should.NotBeNull(provider, nameof(provider));
            var tokens = provider.GetTokens();
            for (var i = 0; i < tokens.Count; i++)
                tokens[i].Dispose();
        }

        #endregion

        #region Messenger

        public static IMessengerSubscriber SubscribeWeak<TTarget, TMessage>(this IMessenger messenger, TTarget target, Action<TTarget, object, TMessage, IMessengerContext> action, ThreadExecutionMode? executionMode = null)
            where TTarget : class
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<TTarget, TMessage>(target, action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber SubscribeWeak<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action, ThreadExecutionMode? executionMode = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<object, TMessage>(action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber Subscribe<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action, ThreadExecutionMode? executionMode = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new DelegateMessengerSubscriber<TMessage>(action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber Subscribe(this IMessenger messenger, IMessengerHandler handler, ThreadExecutionMode? executionMode = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new MessengerHandlerSubscriber(handler);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static void UnsubscribeAll(this IMessenger messenger)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            foreach (var subscriber in messenger.GetSubscribers())
                messenger.Unsubscribe(subscriber);
        }

        #endregion

        #region Common

        public static void RemoveAllListeners<T>(this IHasEventListener<T> hasEventListener) where T : class
        {
            Should.NotBeNull(hasEventListener, nameof(hasEventListener));
            foreach (var listener in hasEventListener.GetListeners())
                hasEventListener.RemoveListener(listener);
        }

        [Pure]
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            return (T)serviceProvider.GetService(typeof(T));
        }

        public static WeakReference GetWeakReference(object item, bool ignoreHasWeakReference = false)
        {
            if (!ignoreHasWeakReference && item is IHasWeakReference hasWeakReference)
                return hasWeakReference.WeakReference;
            return Singleton<IWeakReferenceFactory>.Instance.CreateWeakReference(item!);
        }

        public static TResult[] ToArray<T, TResult>(this IReadOnlyCollection<T> collection, Func<T, TResult> selector)
        {
            Should.NotBeNull(collection, nameof(collection));
            var count = collection.Count;
            if (count == 0)
                return Default.EmptyArray<TResult>();
            var array = new TResult[count];
            count = 0;
            foreach (var item in collection)
                array[count++] = selector(item);
            return array;
        }

        public static T[] ToArray<T>(this IReadOnlyCollection<T> collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            var count = collection.Count;
            if (count == 0)
                return Default.EmptyArray<T>();
            var array = new T[count];
            count = 0;
            foreach (var item in collection)
                array[count++] = item;
            return array;
        }

        //note for better performance use this method for creating delegate instead of handler.Execute because it will use ldftn opcode instead of ldvirtftn       
        public static void ExecuteDelegate(this IThreadDispatcherHandler handler, object? state)
        {
            handler.Execute(state);
        }

        //note for better performance use this method for creating delegate if state parameter is null
        public static void ExecuteNullState(this IThreadDispatcherHandler handler)
        {
            handler.Execute(null);
        }

        internal static bool LazyInitialize<T>(ref T item, T value) where T : class ?
        {
            return Interlocked.CompareExchange(ref item, value, null) == null;
        }

        internal static bool LazyInitializeLock<TTarget, TValue>(ref TValue item, TTarget target, Func<TTarget, TValue> getValue, object locker)
            where TTarget : class
            where TValue : class ?
        {
            if (item != null)
                return false;
            lock (locker)
            {
                if (item != null)
                    return false;
                item = getValue(target);
                return true;
            }
        }

        #endregion

        #endregion
    }
}