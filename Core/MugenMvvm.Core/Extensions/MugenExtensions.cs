using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class MugenExtensions
    {
        #region Fields

        private static Action<object>? _notNullValidateAction;

        #endregion

        #region Collections

        public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TKey, TValue>(this Dictionary<TKey, TValue>.ValueCollection? collection)
        {
            if (collection == null)
                return Default.EmptyArray<TValue>();
            if (collection is IReadOnlyCollection<TValue> readOnlyCollection)
                return readOnlyCollection;
            return collection.ToList();
        }

        #endregion

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

        public static IMessengerSubscriber SubscribeWeak<TTarget, TMessage>(this IMessenger messenger, TTarget target, Action<TTarget, object, TMessage, IMessengerContext> action,
            ThreadExecutionMode? executionMode = null)
            where TTarget : class
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<TTarget, TMessage>(target, action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber SubscribeWeak<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action,
            ThreadExecutionMode? executionMode = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<object, TMessage>(action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber Subscribe<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action,
            ThreadExecutionMode? executionMode = null)
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
                messenger.Unsubscribe(subscriber.Subscriber);
        }

        #endregion

        #region Tracer

        public static void Info(this ITracer tracer, string message)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            tracer.Trace(TraceLevel.Information, message);
        }

        public static void Warn(this ITracer tracer, string message)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            tracer.Trace(TraceLevel.Warning, message);
        }

        public static void Error(this ITracer tracer, string message)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            tracer.Trace(TraceLevel.Error, message);
        }

        [StringFormatMethod("format")]
        public static void Trace(this ITracer tracer, TraceLevel level, string format, params object[] args)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            if (tracer.CanTrace(level))
                tracer.Trace(level, format.Format(args));
        }

        [StringFormatMethod("format")]
        public static void Info(this ITracer tracer, string format, params object[] args)
        {
            tracer.Trace(TraceLevel.Information, format, args);
        }

        [StringFormatMethod("format")]
        public static void Warn(this ITracer tracer, string format, params object[] args)
        {
            tracer.Trace(TraceLevel.Warning, format, args);
        }

        [StringFormatMethod("format")]
        public static void Error(this ITracer tracer, string format, params object[] args)
        {
            tracer.Trace(TraceLevel.Error, format, args);
        }

        #endregion

        #region Metadata

        public static T Get<T>(this IReadOnlyMetadataContext metadataContext, IMetadataContextKey<T> key, T defaultValue = default)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.TryGet(key, out var value, defaultValue);
            return value;
        }

        public static MetadataContextKey.Builder<T> NotNull<T>(this MetadataContextKey.Builder<T> builder) where T : class
        {
            if (_notNullValidateAction == null)
                _notNullValidateAction = value => Should.NotBeNull(value, nameof(value));
            return builder.WithValidation(_notNullValidateAction);
        }

        #endregion

        #region Common

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args)
        {
            Should.NotBeNull(format, nameof(format));
            Should.NotBeNull(args, nameof(args));
            return string.Format(format, args);
        }

        public static void RemoveAllListeners<T>(this IHasListeners<T> hasListeners) where T : class
        {
            Should.NotBeNull(hasListeners, nameof(hasListeners));
            foreach (var listener in hasListeners.GetListeners())
                hasListeners.RemoveListener(listener);
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

        #endregion

        #region Exceptions

        [Pure]
        public static string Flatten(this Exception exception, bool includeStackTrace = false)
        {
            return exception.Flatten(string.Empty, includeStackTrace);
        }

        [Pure]
        public static string Flatten(this Exception exception, string message, bool includeStackTrace = false)
        {
            Should.NotBeNull(exception, nameof(exception));
            var sb = new StringBuilder(message);
            FlattenInternal(exception, sb, includeStackTrace);
            return sb.ToString();
        }

        private static void FlattenInternal(Exception exception, StringBuilder sb, bool includeStackTrace)
        {
            if (exception is AggregateException aggregateException)
            {
                sb.AppendLine(aggregateException.Message);
                if (includeStackTrace)
                {
                    sb.Append(exception.StackTrace);
                    sb.AppendLine();
                }

                for (var index = 0; index < aggregateException.InnerExceptions.Count; index++)
                    FlattenInternal(aggregateException.InnerExceptions[index], sb, includeStackTrace);
                return;
            }

            while (exception != null)
            {
                sb.AppendLine(exception.Message);
                if (includeStackTrace)
                    sb.Append(exception.StackTrace);

                if (exception is ReflectionTypeLoadException loadException)
                {
                    if (includeStackTrace)
                        sb.AppendLine();
                    for (var index = 0; index < loadException.LoaderExceptions.Length; index++)
                        FlattenInternal(loadException.LoaderExceptions[index], sb, includeStackTrace);
                }

                exception = exception.InnerException;
                if (exception != null && includeStackTrace)
                    sb.AppendLine();
            }
        }

        #endregion

        #region Internal

        internal static IList<T>? ToSerializable<T>(this IReadOnlyList<T>? items, ISerializer serializer, int? size = null)
        {
            if (items == null)
                return null;
            List<T> result = null;
            for (var i = 0; i < size.GetValueOrDefault(items.Count); i++)
            {
                var listener = items[i];
                if (serializer.CanSerialize(listener.GetType()))
                {
                    if (result == null)
                        result = new List<T>();
                    result.Add(listener);
                }
            }

            return result;
        }

        internal static bool HasFlagEx(this BusyMessageHandlerType handlerMode, BusyMessageHandlerType value)
        {
            return (handlerMode & value) == value;
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
    }
}