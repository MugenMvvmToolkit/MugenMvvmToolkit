using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Infrastructure.Navigation;
using MugenMvvm.Infrastructure.Wrapping;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class MugenExtensions//todo split
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

        public static TTask WithBusyIndicator<TTask>(this TTask task, IHasService<IBusyIndicatorProvider> busyIndicatorProvider, object? message = null, int millisecondsDelay = 0)
            where TTask : Task
        {
            Should.NotBeNull(busyIndicatorProvider, nameof(busyIndicatorProvider));
            return task.WithBusyIndicator(busyIndicatorProvider.Service, message, millisecondsDelay);
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

        public static IMessengerSubscriber SubscribeWeak<TTarget, TMessage>(this IMessenger messenger, TTarget target, Action<TTarget, object, TMessage, IMessengerContext> action, ThreadExecutionMode executionMode)
            where TTarget : class
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<TTarget, TMessage>(target, action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber SubscribeWeak<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<object, TMessage>(action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber Subscribe<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new DelegateMessengerSubscriber<TMessage>(action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber Subscribe(this IMessenger messenger, IMessengerHandler handler, ThreadExecutionMode executionMode)
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
            metadata.AddOrUpdate(key, handler, (object?)null, (object?)null, (item, value, currentValue, state1, state2) => (T)Delegate.Combine(currentValue, value));
        }

        public static void RemoveHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T> key, T handler)
            where T : Delegate
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object?)null, (object?)null, (item, value, currentValue, state1, state2) => (T)Delegate.Remove(currentValue, value));
        }

        public static T Get<T>(this IReadOnlyMetadataContext metadataContext, IMetadataContextKey<T> key, T defaultValue = default)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.TryGet(key, out var value, defaultValue);
            return value;
        }

        public static MetadataContextKey.Builder<T> NotNull<T>(this MetadataContextKey.Builder<T> builder) where T : class ?
        {
            if (_notNullValidateAction == null)
                _notNullValidateAction = (ctx, k, value) => Should.NotBeNull(value, nameof(value));
            return builder.WithValidation(_notNullValidateAction!);
        }

        #endregion

        #region Navigation

        public static void RegisterMediatorFactory<TMediator, TView>(this INavigationMediatorViewModelPresenter viewModelPresenter, bool disableWrap = false, int priority = 0)
            where TMediator : NavigationMediatorBase<TView>
            where TView : class
        {
            RegisterMediatorFactory(viewModelPresenter, typeof(TMediator), typeof(TView), disableWrap, priority);
        }

        public static void RegisterMediatorFactory(this INavigationMediatorViewModelPresenter viewModelPresenter, Type mediatorType, Type viewType, bool disableWrap, int priority = 0)
        {
            Should.NotBeNull(viewModelPresenter, nameof(viewModelPresenter));
            Should.NotBeNull(mediatorType, nameof(mediatorType));
            Should.NotBeNull(viewType, nameof(viewType));
            if (disableWrap)
            {
                viewModelPresenter.Managers.Add(new DelegateNavigationMediatorFactory((vm, initializer, arg3) =>
                {
                    if (initializer.ViewType.EqualsEx(viewType))
                        return (INavigationMediator)Service<IServiceProvider>.Instance.GetService(mediatorType);
                    return null;
                }, priority));
            }
            else
            {
                viewModelPresenter.Managers.Add(new DelegateNavigationMediatorFactory((vm, initializer, arg3) =>
                {
                    if (viewType.IsAssignableFromUnified(initializer.ViewType) || Service<IWrapperManager>.Instance.CanWrap(initializer.ViewType, viewType, arg3))
                        return (INavigationMediator)Service<IServiceProvider>.Instance.GetService(mediatorType);
                    return null;
                }, priority));
            }
        }

        public static INavigatingResult OnNavigatingTo(this INavigationDispatcher navigationDispatcher, INavigationProvider navigationProvider, NavigationMode mode,
             NavigationType navigationType, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            return navigationDispatcher.OnNavigating(navigationDispatcher.ContextFactory.GetNavigationContextTo(navigationProvider, mode, navigationType, viewModel, metadata ?? Default.MetadataContext));
        }

        public static INavigatingResult OnNavigatingFrom(this INavigationDispatcher navigationDispatcher, INavigationProvider navigationProvider, NavigationMode mode,
            NavigationType navigationType, IViewModelBase viewModelFrom, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            return navigationDispatcher.OnNavigating(navigationDispatcher.ContextFactory.GetNavigationContextFrom(navigationProvider, mode, navigationType, viewModelFrom,
                metadata ?? Default.MetadataContext));
        }

        public static Task WaitNavigationAsync(this INavigationDispatcher navigationDispatcher, Func<INavigationCallback, bool> filter,
            IReadOnlyMetadataContext? metadata = null)
        {
            return navigationDispatcher?.NavigationJournal.WaitNavigationAsync(filter, metadata);
        }

        public static Task WaitNavigationAsync(this INavigationDispatcherJournal navigationDispatcherJournal, Func<INavigationCallback, bool> filter, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationDispatcherJournal, nameof(navigationDispatcherJournal));
            Should.NotBeNull(filter, nameof(filter));
            if (metadata == null)
                metadata = Default.MetadataContext;
            var entries = navigationDispatcherJournal.GetNavigationEntries(null, metadata);
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

        public static IWrapperManagerFactory AddWrapper(this IWrapperManager wrapperManager, Func<IWrapperManager, Type, Type, IReadOnlyMetadataContext, bool> condition,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, object?> wrapperFactory)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var factory = new DelegateWrapperManagerFactory(condition, wrapperFactory);
            wrapperManager.WrapperFactories.Add(factory);
            return factory;
        }

        public static IWrapperManagerFactory AddWrapper(this IWrapperManager wrapperManager, Type wrapperType, Type implementation,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, object>? wrapperFactory = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.BeOfType(implementation, nameof(implementation), wrapperType);
            if (implementation.IsInterfaceUnified() || implementation.IsAbstractUnified())
                ExceptionManager.ThrowWrapperTypeShouldBeNonAbstract(implementation);

            if (wrapperFactory == null)
            {
                var constructor = implementation
                    .GetConstructorsUnified(MemberFlags.InstanceOnly)
                    .FirstOrDefault();
                if (constructor == null)
                    ExceptionManager.ThrowCannotFindConstructor(implementation);

                wrapperFactory = (manager, o, arg3, arg4) => constructor.InvokeEx(o);
            }
            return wrapperManager.AddWrapper((manager, type, arg3, arg4) => wrapperType.EqualsEx(arg3), wrapperFactory);
        }

        public static IWrapperManagerFactory AddWrapper<TWrapper>(this IWrapperManager wrapperManager, Type implementation,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
            where TWrapper : class
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), implementation, wrapperFactory);
        }

        public static IWrapperManagerFactory AddWrapper<TWrapper, TImplementation>(this IWrapperManager wrapperManager,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
            where TWrapper : class
            where TImplementation : class, TWrapper
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), typeof(TImplementation), wrapperFactory);
        }

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args)
        {
            return string.Format(format, args);
        }

        public static void AddListener<T>(this IHasListeners<T> hasListeners, T listener) where T : class, IListener
        {
            Should.NotBeNull(hasListeners, nameof(hasListeners));
            hasListeners.Listeners.Add(listener);
        }

        public static void RemoveListener<T>(this IHasListeners<T> hasListeners, T listener) where T : class, IListener
        {
            Should.NotBeNull(hasListeners, nameof(hasListeners));
            hasListeners.Listeners.Remove(listener);
        }

        public static void RemoveAllListeners<T>(this IHasListeners<T> hasListeners) where T : class, IListener
        {
            Should.NotBeNull(hasListeners, nameof(hasListeners));
            hasListeners.Listeners.Clear();
        }

        [Pure]
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            return (T)serviceProvider.GetService(typeof(T));
        }

        [Pure]
        public static bool TryGetService<T>(this IServiceProvider serviceProvider, [NotNullWhenTrue] out T service) where T : class
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            try
            {
                if (serviceProvider is IServiceProviderEx serviceProviderEx)
                {
                    if (serviceProviderEx.TryGetService(typeof(T), out var o))
                    {
                        service = (T)o!;
                        return true;
                    }
                    service = default!;
                    return false;
                }

                service = (T)serviceProvider.GetService(typeof(T));
                return true;
            }
            catch
            {
                service = default!;
                return false;
            }
        }

        public static WeakReference GetWeakReference(object? item, bool ignoreHasWeakReference = false)
        {
            if (item == null)
                return Default.WeakReference;
            if (!ignoreHasWeakReference && item is IHasWeakReference hasWeakReference)
                return hasWeakReference.WeakReference;
            return Service<IWeakReferenceFactory>.Instance.GetWeakReference(item!);
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

        private class ViewWrappersCollection : List<object>, IObservableMetadataContextListener
        {
            public ViewWrappersCollection(IObservableMetadataContext metadata)
            {
                metadata.Listeners.Add(this);
            }

            public void OnAdded(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? newValue)
            {

            }

            public void OnChanged(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue)
            {
                if (ViewMetadata.Wrappers.Equals(key) && !ReferenceEquals(newValue, this))
                    TryDispose();
            }

            public void OnRemoved(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
            {
                if (ViewMetadata.Wrappers.Equals(key))
                    TryDispose();
            }

            private void TryDispose()
            {
                foreach (var item in this.OfType<IDisposable>())
                    item.Dispose();
            }

            public int GetPriority(object source)
            {
                return 0;
            }
        }

        public static TView? TryWrap<TView>(this IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
            where TView : class
        {
            return (TView?)viewInfo.TryWrap(typeof(TView), metadata);
        }

        public static object? TryWrap(this IViewInfo viewInfo, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            return WrapInternal(viewInfo, wrapperType, metadata, true);
        }

        public static TView Wrap<TView>(this IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
            where TView : class
        {
            return (TView)viewInfo.Wrap(typeof(TView), metadata);
        }

        public static object Wrap(this IViewInfo viewInfo, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            return WrapInternal(viewInfo, wrapperType, metadata, false)!;
        }

        public static bool CanWrap<TView>(this IViewInfo viewInfo, IReadOnlyMetadataContext metadata) where TView : class
        {
            return viewInfo.CanWrap(typeof(TView), metadata);
        }

        public static bool CanWrap(this IViewInfo viewInfo, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            return wrapperType.IsInstanceOfTypeUnified(viewInfo.View) || Service<IWrapperManager>.Instance.CanWrap(viewInfo.View.GetType(), wrapperType, metadata);
        }

        private static object? WrapInternal(this IViewInfo viewInfo, Type wrapperType, IReadOnlyMetadataContext metadata, bool checkCanWrap)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsInstanceOfTypeUnified(viewInfo.View))
                return viewInfo.View;

            var collection = viewInfo.Metadata.GetOrAdd(ViewMetadata.Wrappers, viewInfo, viewInfo, (context, _, __) => new ViewWrappersCollection((IObservableMetadataContext)context));
            lock (collection)
            {
                var item = collection.FirstOrDefault(new Func<object, bool>(wrapperType.IsInstanceOfTypeUnified));
                if (item == null)
                {
                    var wrapperManager = Service<IWrapperManager>.Instance;
                    if (checkCanWrap && !wrapperManager.CanWrap(viewInfo.View.GetType(), wrapperType, metadata))
                        return null;

                    item = wrapperManager.Wrap(viewInfo.View, wrapperType, metadata);
                    collection.Add(item);
                }

                return item;
            }
        }

        #endregion

        #region View models

        public static bool TrySubscribe(this IViewModelBase viewModel, object observer, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(observer, nameof(observer));
            return Service<IViewModelDispatcher>.Instance.Subscribe(viewModel, observer, executionMode ?? ThreadExecutionMode.Current, metadata ?? Default.MetadataContext);
        }

        public static TService? TryGetService<TService>(this IViewModelBase viewModel) where TService : class
        {
            if (viewModel is IHasService<TService> hasService)
                return hasService.Service;
            return null;
        }

        public static TService? TryGetServiceOptional<TService>(this IViewModelBase viewModel) where TService : class
        {
            if (viewModel is IHasServiceOptional<TService> hasServiceOptional)
                return hasServiceOptional.ServiceOptional;
            return viewModel.TryGetService<TService>();
        }

        public static void InvalidateCommands<TViewModel>(this TViewModel viewModel) where TViewModel : class, IViewModelBase, IHasService<IEventPublisher>
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            viewModel.Service.Publish(viewModel, Default.EmptyPropertyChangedArgs);
        }

        public static bool IsDisposed(this IViewModelBase viewModel)
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
            List<T>? result = null;
            for (var i = 0; i < size.GetValueOrDefault(items.Count); i++)
            {
                var listener = items[i];
                if (listener != null && serializer.CanSerialize(listener.GetType(), Default.MetadataContext))
                {
                    if (result == null)
                        result = new List<T>();
                    result.Add(listener);
                }
            }

            return result;
        }

        internal static bool HasFlagEx(this BatchUpdateCollectionMode mode, BatchUpdateCollectionMode value)
        {
            return (mode & value) == value;
        }

        internal static bool HasFlagEx(this BusyMessageHandlerType handlerMode, BusyMessageHandlerType value)
        {
            return (handlerMode & value) == value;
        }

        internal static bool LazyInitialize<T>([EnsuresNotNull]ref IComponentCollection<T>? item, object target, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            return item == null && LazyInitialize(ref item, Service<IComponentCollectionFactory>.Instance.GetComponentCollection<T>(target, metadata ?? Default.MetadataContext));
        }

        internal static bool LazyInitialize<T>([EnsuresNotNull]ref T? item, T value) where T : class
        {
            return Interlocked.CompareExchange(ref item, value, null) == null;
        }

        internal static bool LazyInitializeDisposable<T>([EnsuresNotNull]ref T? item, T value) where T : class, IDisposable
        {
            if (!LazyInitialize(ref item, value))
            {
                value.Dispose();
                return false;
            }

            return true;
        }

        internal static bool LazyInitializeLock<TTarget, TValue>([EnsuresNotNull]ref TValue? item, TTarget target, Func<TTarget, TValue> getValue, object locker)
            where TValue : class
            where TTarget : class
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

        public static IWeakEventHandler<TArg> CreateWeakEventHandler<TTarget, TArg>(TTarget target, Action<TTarget, object, TArg> invokeAction, Action<object, IWeakEventHandler<TArg>>? unsubscribeAction = null)
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

            public TDelegate? HandlerDelegate;
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
                            action.Invoke(sender, HandlerDelegate!);
                    }
                }
                else
                    _invokeAction(target, sender, arg);
            }

            #endregion
        }
    }
}