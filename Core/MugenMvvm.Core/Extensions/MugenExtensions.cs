using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Infrastructure.Navigation;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Interfaces.Wrapping;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class MugenExtensions
    {
        #region Fields

        private static Action<IReadOnlyMetadataContext, object, object>? _notNullValidateAction;

        #endregion

        #region Collections

        public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TKey, TValue>(this Dictionary<TKey, TValue>.ValueCollection? collection)
        {
            if (collection == null)
                return Default.EmptyArray<TValue>();
            if (collection is IReadOnlyCollection<TValue> readOnlyCollection)
                return readOnlyCollection;
            return collection.ToArray();
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
        public static void Trace(this ITracer tracer, TraceLevel level, string format, params object?[] args)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            if (tracer.CanTrace(level))
                tracer.Trace(level, format.Format(args));
        }

        [StringFormatMethod("format")]
        public static void Info(this ITracer tracer, string format, params object?[] args)
        {
            tracer.Trace(TraceLevel.Information, format, args);
        }

        [StringFormatMethod("format")]
        public static void Warn(this ITracer tracer, string format, params object?[] args)
        {
            tracer.Trace(TraceLevel.Warning, format, args);
        }

        [StringFormatMethod("format")]
        public static void Error(this ITracer tracer, string format, params object?[] args)
        {
            tracer.Trace(TraceLevel.Error, format, args);
        }

        #endregion

        #region Metadata

        public static void AddHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T> key, T handler)
            where T : Delegate
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object)null, (object)null, (item, value, currentValue, state1, state2) => (T)Delegate.Combine(currentValue, value));
        }

        public static void RemoveHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T> key, T handler)
            where T : Delegate
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object)null, (object)null, (item, value, currentValue, state1, state2) => (T)Delegate.Remove(currentValue, value));
        }

        public static T Get<T>(this IReadOnlyMetadataContext metadataContext, IMetadataContextKey<T> key, T defaultValue = default)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.TryGet(key, out var value, defaultValue);
            return value;
        }

        public static MetadataContextKey.Builder<T> NotNull<T>(this MetadataContextKey.Builder<T> builder) where T : class
        {
            if (_notNullValidateAction == null)
                _notNullValidateAction = (ctx, k, value) => Should.NotBeNull(value, nameof(value));
            return builder.WithValidation(_notNullValidateAction);
        }

        #endregion

        #region Navigation

        public static Task WaitNavigationAsync(this INavigationDispatcher navigationDispatcher, Func<INavigationCallback, bool> filter, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(filter, nameof(filter));
            if (metadata == null)
                metadata = Default.MetadataContext;
            var entries = navigationDispatcher.GetNavigationEntries(null, metadata);
            List<Task>? tasks = null;
            for (int i = 0; i < entries.Count; i++)
            {
                var callbacks = entries[i].GetCallbacks(null, metadata);
                for (int j = 0; j < callbacks.Count; j++)
                {
                    if (tasks == null)
                        tasks = new List<Task>();
                    var callback = callbacks[i];
                    if (filter(callback))
                        tasks.Add(callback.WaitAsync());
                }
            }

            if (tasks == null)
                return Default.CompletedTask;
            return Task.WhenAll(tasks);
        }

        public static INavigationContext CreateNavigateToContext(this INavigationDispatcher navigationDispatcher, INavigationProvider navigationProvider, NavigationType navigationType, IViewModel viewModelTo, NavigationMode mode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(viewModelTo, nameof(viewModelTo));
            var entry = navigationDispatcher.GetLastNavigationEntry(navigationType, metadata: metadata);
            var context = new NavigationContext(navigationProvider, navigationType, mode, entry?.ViewModel, viewModelTo, metadata);
            if (entry != null && entry.NavigationType != navigationType)
                context.Metadata.Set(NavigationInternalMetadata.ViewModelFromNavigationType, entry.NavigationType);
            return context;
        }

        public static INavigationContext CreateNavigateFromContext(this INavigationDispatcher navigationDispatcher, INavigationProvider navigationProvider, NavigationType navigationType, IViewModel viewModelFrom, NavigationMode mode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(viewModelFrom, nameof(viewModelFrom));
            var entry = navigationDispatcher.GetLastNavigationEntry(navigationType, metadata: metadata);
            var context = new NavigationContext(navigationProvider, navigationType, mode, viewModelFrom, entry?.ViewModel, metadata);
            if (entry != null && entry.NavigationType != navigationType)
                context.Metadata.Set(NavigationInternalMetadata.ViewModelToNavigationType, entry.NavigationType);
            return context;
        }

        public static INavigatingResult OnNavigatingTo(this INavigationDispatcher navigationDispatcher, INavigationProvider navigationProvider, NavigationType navigationType, IViewModel viewModelTo, NavigationMode mode, IReadOnlyMetadataContext? metadata = null)
        {
            return navigationDispatcher.OnNavigating(navigationDispatcher.CreateNavigateToContext(navigationProvider, navigationType, viewModelTo, mode, metadata));
        }

        public static INavigatingResult OnNavigatingFrom(this INavigationDispatcher navigationDispatcher, INavigationProvider navigationProvider, NavigationType navigationType, IViewModel viewModelFrom, NavigationMode mode, IReadOnlyMetadataContext? metadata = null)
        {
            return navigationDispatcher.OnNavigating(navigationDispatcher.CreateNavigateFromContext(navigationProvider, navigationType, viewModelFrom, mode, metadata));
        }

        public static INavigationEntry? GetLastNavigationEntry(this INavigationDispatcher navigationDispatcher, NavigationType navigationType, Func<INavigationEntry, bool>? filter = null, IReadOnlyMetadataContext? metadata = null)
        {//todo rewrite bug navigationType!
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationType, nameof(navigationType));
            if (filter == null)
                filter = entry => entry.NavigationType != NavigationType.Tab;
            var entries = navigationDispatcher.GetNavigationEntries(null, metadata ?? Default.MetadataContext);
            return entries.Where(filter).OrderByDescending(entry => entry.NavigationDate).FirstOrDefault();
        }

        #endregion

        #region Common

        public static Task<T> TaskFromException<T>(Exception exception)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.TrySetExceptionEx(exception);
            return tcs.Task;
        }

        public static string Dump(this IReadOnlyMetadataContext? metadata)
        {
            if (metadata == null)
                return string.Empty;
            var builder = new StringBuilder("(");
            var values = metadata.ToArray();
            foreach (var item in values)
                builder.Append(item.ContextKey.Key).Append("=").Append(item.Value).Append(";");
            builder.Append(")");
            return builder.ToString();
        }

        public static void AddWrapper<TWrapper>(this IConfigurableWrapperManager wrapperManager, Type implementation,
            Func<Type, IReadOnlyMetadataContext, bool>? condition = null, Func<object, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
            where TWrapper : class
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            wrapperManager.AddWrapper(typeof(TWrapper), implementation, condition, wrapperFactory);
        }

        public static void AddWrapper<TWrapper, TImplementation>(this IConfigurableWrapperManager wrapperManager, Func<Type, IReadOnlyMetadataContext, bool>? condition = null,
            Func<object, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
            where TWrapper : class
            where TImplementation : class, TWrapper
        {

            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            wrapperManager.AddWrapper(typeof(TImplementation), condition, wrapperFactory);
        }

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

        [Pure]
        public static bool TryGetService<T>(this IServiceProvider serviceProvider, out T service)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            try
            {
                if (serviceProvider is IServiceProviderEx serviceProviderEx)
                {
                    if (serviceProviderEx.TryGetService(typeof(T), out var o))
                    {
                        service = (T)o;
                        return true;
                    }
                    service = default;
                    return false;
                }

                service = (T)serviceProvider.GetService(typeof(T));
                return true;
            }
            catch
            {
                service = default;
                return false;
            }
        }

        public static WeakReference GetWeakReference(object? item, bool ignoreHasWeakReference = false)
        {
            if (item == null)
                return Default.WeakReference;
            if (!ignoreHasWeakReference && item is IHasWeakReference hasWeakReference)
                return hasWeakReference.WeakReference;
            return Service<IWeakReferenceFactory>.Instance.CreateWeakReference(item!);
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

        #region Views

        public static TView GetUnderlyingView<TView>(this IView? view)
            where TView : class ?
        {
            return GetUnderlyingView<TView>(viewObj: view);
        }

        public static TView GetUnderlyingView<TView>(object? viewObj)
            where TView : class ?
        {
            while (true)
            {
                var wrapper = viewObj as IWrapperView;
                if (wrapper?.View == null || wrapper.View == viewObj)
                    return (TView)viewObj;
                viewObj = wrapper.View;
            }
        }

        #endregion

        #region View models

        public static void InvalidateCommands(this IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            viewModel.Publish(viewModel, Default.EmptyPropertyChangedArgs);
        }

        public static bool IsDisposed(this IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return viewModel.Metadata.Get(ViewModelMetadata.LifecycleState).IsDispose;
        }

        #endregion

        #region Internal

        internal static void TrySetExceptionEx<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            if (e is AggregateException aggregateException)
                tcs.TrySetException(aggregateException.InnerExceptions);
            else
                tcs.SetException(e);
        }

        internal static List<T>? ToSerializable<T>(this IReadOnlyList<T>? items, ISerializer serializer, int? size = null)
        {
            if (items == null)
                return null;
            List<T> result = null;
            for (var i = 0; i < size.GetValueOrDefault(items.Count); i++)
            {
                var listener = items[i];
                if (serializer.CanSerialize(listener.GetType(), Default.MetadataContext))
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

        internal static bool LazyInitializeDisposable<T>(ref T item, T value) where T : class ?, IDisposable
        {
            if (!LazyInitialize(ref item, value))
            {
                value.Dispose();
                return false;
            }

            return true;
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

        private static readonly Action<object, PropertyChangedEventHandler> UnsubscribePropertyChangedDelegate;
        private static readonly Func<IWeakEventHandler<PropertyChangedEventArgs>, PropertyChangedEventHandler> CreatePropertyChangedHandlerDelegate;

        public static IWeakEventHandler<TArg> CreateWeakEventHandler<TTarget, TArg>(TTarget target, Action<TTarget, object, TArg> invokeAction, Action<object, IWeakEventHandler<TArg>> unsubscribeAction = null)
            where TTarget : class
        {
            return new WeakEventHandler<TTarget, TArg, object>(target, invokeAction, unsubscribeAction);
        }

        public static TResult CreateWeakDelegate<TTarget, TArg, TResult>(TTarget target, Action<TTarget, object, TArg> invokeAction, Action<object, TResult>? unsubscribeAction, Func<IWeakEventHandler<TArg>, TResult> createHandler)
            where TTarget : class
            where TResult : class
        {
            Should.NotBeNull(createHandler, nameof(createHandler));
            var weakEventHandler = new WeakEventHandler<TTarget, TArg, TResult>(target, invokeAction, unsubscribeAction);
            var handler = createHandler(weakEventHandler);
            if (unsubscribeAction == null)
                return handler;
            weakEventHandler.HandlerDelegate = handler;
            return handler;
        }

        public static PropertyChangedEventHandler MakeWeakPropertyChangedHandler<TTarget>(TTarget target, Action<TTarget, object, PropertyChangedEventArgs> invokeAction)
            where TTarget : class
        {
            return CreateWeakDelegate(target, invokeAction, UnsubscribePropertyChangedDelegate, CreatePropertyChangedHandlerDelegate);
        }

        public interface IWeakEventHandler<in TArg>
        {
            void Handle(object sender, TArg arg);
        }

        private sealed class WeakEventHandler<TTarget, TArg, TDelegate> : IWeakEventHandler<TArg>
            where TTarget : class
            where TDelegate : class
        {
            #region Fields

            public TDelegate HandlerDelegate;
            private readonly WeakReference _targetReference;
            private readonly Action<TTarget, object, TArg> _invokeAction;
            private readonly Delegate? _unsubscribeAction;

            #endregion

            #region Constructors

            public WeakEventHandler(TTarget target, Action<TTarget, object, TArg> invokeAction, Delegate? unsubscribeAction)
            {
                Should.NotBeNull(target, nameof(target));
                Should.NotBeNull(invokeAction, nameof(invokeAction));
                _invokeAction = invokeAction;
                _unsubscribeAction = unsubscribeAction;
                _targetReference = GetWeakReference(target);
            }

            #endregion

            #region Methods

            public void Handle(object sender, TArg arg)
            {
                var target = (TTarget)_targetReference.Target;
                if (target == null)
                {
                    if (_unsubscribeAction != null)
                    {
                        var action = _unsubscribeAction as Action<object, TDelegate>;
                        if (action == null)
                            ((Action<object, IWeakEventHandler<TArg>>)_unsubscribeAction).Invoke(sender, this);
                        else
                            action.Invoke(sender, HandlerDelegate);
                    }
                }
                else
                    _invokeAction(target, sender, arg);
            }

            #endregion
        }
    }
}