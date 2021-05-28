using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Collections;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Threading;
using MugenMvvm.Wrapping.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        private static IReadOnlyMetadataContext? _forceExecuteMetadata;

        public static void Start<TViewModel>(this IMugenApplication? _, IReadOnlyMetadataContext? metadata = null)
            where TViewModel : IViewModelBase => Start(null, typeof(TViewModel), metadata);

        public static void Start(this IMugenApplication? _, Type viewModelType, IReadOnlyMetadataContext? metadata = null)
        {
            if (metadata.IsNullOrEmpty())
                metadata = NavigationMetadata.Modal.ToContext(false);
            else
            {
                var context = metadata.ToNonReadonly();
                context.Set(NavigationMetadata.Modal, false);
                metadata = context;
            }

            var vm = MugenService.ViewModelManager.GetViewModel(viewModelType, metadata);
            vm.ShowAsync(default, metadata).CloseCallback.AddCallback(NavigationCallbackDelegateListener.DisposeTargetCallback);
        }

        public static IBusyToken BeginBusy(this IBusyManager busyManager, object? request = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            var token = busyManager.TryBeginBusy(request, metadata);
            if (token == null)
                ExceptionManager.ThrowRequestNotSupported<IBusyManagerComponent>(busyManager, request, metadata);
            return token;
        }

        public static void ClearBusy(this IBusyManager busyManager)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            foreach (var t in busyManager.GetTokens())
                t.Dispose();
        }

        public static IEntityTrackingCollection GetTrackingCollection(this IEntityManager entityManager, object? request = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(entityManager, nameof(entityManager));
            var collection = entityManager.TryGetTrackingCollection(request, metadata);
            if (collection == null)
                ExceptionManager.ThrowRequestNotSupported<IEntityTrackingCollectionProviderComponent>(entityManager, request, metadata);
            return collection;
        }

        public static IEntityStateSnapshot GetSnapshot(this IEntityManager entityManager, object entity, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(entityManager, nameof(entityManager));
            var snapshot = entityManager.TryGetSnapshot(entity, metadata);
            if (snapshot == null)
                ExceptionManager.ThrowRequestNotSupported<IEntityStateSnapshotProviderComponent>(entityManager, entity, metadata);
            return snapshot;
        }

        public static TResult Serialize<TRequest, TResult>(this ISerializer serializer, ISerializationFormat<TRequest, TResult> format, TRequest request,
            TResult? buffer = default, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            if (!serializer.TrySerialize(format, request, ref buffer, metadata))
                ExceptionManager.ThrowRequestNotSupported<ISerializerComponent<TRequest, TResult>>(serializer, request, metadata);
            return buffer;
        }

        public static TResult Deserialize<TRequest, TResult>(this ISerializer serializer, IDeserializationFormat<TRequest, object?> dynamicFormat, TRequest request,
            TResult? buffer = default, IReadOnlyMetadataContext? metadata = null) where TResult : class =>
            serializer.Deserialize<TRequest, TResult>(format: dynamicFormat, request: request, buffer, metadata: metadata);

        public static TResult Deserialize<TRequest, TResult>(this ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest request,
            TResult? buffer = default, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            if (!serializer.TryDeserialize(format, request, ref buffer, metadata))
                ExceptionManager.ThrowRequestNotSupported<IDeserializerComponent<TRequest, TResult>>(serializer, request, metadata);
            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IWeakReference GetWeakReference(this IWeakReferenceManager weakReferenceManager, object? item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(weakReferenceManager, nameof(weakReferenceManager));
            var result = weakReferenceManager.TryGetWeakReference(item, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IWeakReferenceProviderComponent>(weakReferenceManager, item, metadata);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttachedValueStorage AttachedValues<T>(this T target, IReadOnlyMetadataContext? metadata = null, IAttachedValueManager? attachedValueManager = null)
            where T : class
            => attachedValueManager.DefaultIfNull().TryGetAttachedValues(target, metadata);

        public static void ExecuteRaw(IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (!threadDispatcher.DefaultIfNull().TryExecute(executionMode, handler, state, metadata))
                ExceptionManager.ThrowRequestNotSupported<IThreadDispatcherComponent>(threadDispatcher.DefaultIfNull(), handler, metadata);
        }

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, Action action, IReadOnlyMetadataContext? metadata = null) =>
            ExecuteRaw(threadDispatcher, executionMode, action, null, metadata);

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, Action<object?> action, object? state,
            IReadOnlyMetadataContext? metadata = null) =>
            ExecuteRaw(threadDispatcher, executionMode, action, state, metadata);

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, IThreadDispatcherHandler handler, object? state,
            IReadOnlyMetadataContext? metadata = null) =>
            ExecuteRaw(threadDispatcher, executionMode, handler, state, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ThreadSwitcherAwaitable SwitchToMainAsync(this IThreadDispatcher? threadDispatcher) => threadDispatcher.SwitchToAsync(ThreadExecutionMode.Main);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ThreadSwitcherAwaitable SwitchToBackgroundAsync(this IThreadDispatcher? threadDispatcher) => threadDispatcher.SwitchToAsync(ThreadExecutionMode.Background);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ThreadSwitcherAwaitable SwitchToAsync(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode) =>
            new(threadDispatcher.DefaultIfNull(), executionMode);

        public static ICompositeCommand GetCommand(this ICommandManager? commandManager, object? owner, Action<IReadOnlyMetadataContext?> execute,
            Func<IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null, CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null,
            Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull(owner).GetCommand<object>(owner,
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<T>(this ICommandManager? commandManager, object? owner, Action<T, IReadOnlyMetadataContext?> execute,
            Func<T, IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default, bool? allowMultipleExecution = null, CommandExecutionBehavior? executionMode = null,
            ThreadExecutionMode? eventThreadMode = null,
            Func<object?, object?, bool>? canNotify = null, IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull(owner).GetCommand<T>(owner,
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand(this ICommandManager? commandManager, object? owner, Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute,
            Func<IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null, CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null,
            Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull(owner).GetCommand<object>(owner,
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand(this ICommandManager? commandManager, object? owner, Func<CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> execute,
            Func<IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null, CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null,
            Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull(owner).GetCommand<object>(owner,
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<T>(this ICommandManager? commandManager, object? owner, Func<T, CancellationToken, IReadOnlyMetadataContext?, Task> execute,
            Func<T, IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default, bool? allowMultipleExecution = null, CommandExecutionBehavior? executionMode = null,
            ThreadExecutionMode? eventThreadMode = null,
            Func<object?, object?, bool>? canNotify = null, IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull(owner).GetCommand<T>(owner,
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<T>(this ICommandManager? commandManager, object? owner,
            Func<T, CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> execute,
            Func<T, IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default, bool? allowMultipleExecution = null, CommandExecutionBehavior? executionMode = null,
            ThreadExecutionMode? eventThreadMode = null,
            Func<object?, object?, bool>? canNotify = null, IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull(owner).GetCommand<T>(owner,
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<TParameter>(this ICommandManager commandManager, object? owner, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(commandManager, nameof(commandManager));
            var result = commandManager.TryGetCommand<TParameter>(owner, request, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<ICommandProviderComponent>(commandManager, request, metadata);
            return result;
        }

        public static ActionToken AddNotifier(this ICompositeCommand command, object? notifier, IReadOnlyMetadataContext? metadata = null)
        {
            var handler = command.GetComponentOptional<CommandEventHandler>(metadata);
            if (handler == null)
                return default;
            var token = handler.AddNotifier(notifier, metadata);
            if (!token.IsEmpty && notifier is IHasDisposeCallback hasDisposeCallback)
                hasDisposeCallback.RegisterDisposeToken(token);
            return token;
        }

        public static ValueTask<bool> ForceExecuteAsync(this ICompositeCommand command, object? parameter = null, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(command, nameof(command));
            if (metadata == null)
                metadata = _forceExecuteMetadata ??= CommandMetadata.ForceExecute.ToContext(true);
            else
                metadata = metadata.WithValue(CommandMetadata.ForceExecute, true);
            return command.ExecuteAsync(parameter, cancellationToken, metadata);
        }

        public static object Wrap(this IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var wrapper = wrapperManager.TryWrap(wrapperType, request, metadata);
            if (wrapper == null)
                ExceptionManager.ThrowWrapperTypeNotSupported(wrapperType);
            return wrapper;
        }

        public static IWrapperManagerComponent AddWrapper<TConditionRequest, TWrapRequest>(this IWrapperManager wrapperManager,
            Func<Type, TConditionRequest, IReadOnlyMetadataContext?, bool> condition,
            Func<Type, TWrapRequest, IReadOnlyMetadataContext?, object?> wrapperFactory, int priority = WrappingComponentPriority.WrapperManger,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var wrapper = new DelegateWrapperManager<TConditionRequest, TWrapRequest>(condition, wrapperFactory) {Priority = priority};
            wrapperManager.Components.Add(wrapper, metadata);
            return wrapper;
        }

        public static T? TryUnwrap<T>(object target) where T : class
        {
            while (true)
            {
                if (target is T r)
                    return r;

                if (target is not IWrapper<object> t)
                    return null;

                target = t.Target;
            }
        }

        public static object Unwrap(object target) => Unwrap<object>(target)!;

        public static T? Unwrap<T>(object? target) where T : class
        {
            while (true)
            {
                if (target is not IWrapper<T> t)
                    return target as T;
                target = t.Target;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo CastGeneric<TFrom, TTo>(TFrom value)
        {
            if (typeof(TFrom) == typeof(TTo))
                return ((Func<TFrom, TTo>) (object) GenericCaster<TFrom>.Cast).Invoke(value);
            return (TTo) (object) value!;
        }

        [StringFormatMethod("format")]
        public static string Format(this string format, object? arg0) => string.Format(format, arg0);

        [StringFormatMethod("format")]
        public static string Format(this string format, object? arg0, object? arg1) => string.Format(format, arg0, arg1);

        [StringFormatMethod("format")]
        public static string Format(this string format, object? arg0, object? arg1, object? arg2) => string.Format(format, arg0, arg1, arg2);

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args) => string.Format(format, args);

        [Pure]
        public static string Flatten(this Exception exception, bool includeStackTrace = false) => exception.Flatten(string.Empty, includeStackTrace);

        [Pure]
        public static string Flatten(this Exception exception, string message, bool includeStackTrace = false)
        {
            Should.NotBeNull(exception, nameof(exception));
            var sb = new StringBuilder(message);
            FlattenInternal(exception, sb, includeStackTrace);
            return sb.ToString();
        }

        [return: NotNullIfNotNull("value")]
        public static T EnsureInitialized<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class
            => Volatile.Read(ref item!) ?? Interlocked.CompareExchange(ref item, value, null!) ?? item;

        internal static ActionToken TryLock(object? target)
        {
            if (target is ISynchronizable synchronizable)
                return synchronizable.Lock();
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ReleaseWeakReference(this IValueHolder<IWeakReference>? valueHolder) => valueHolder?.Value?.Release();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IWeakReference ToWeakReference(this object? item) => MugenService.WeakReferenceManager.GetWeakReference(item);

        private static void FlattenInternal(Exception? exception, StringBuilder sb, bool includeStackTrace)
        {
            if (exception == null)
                return;
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

                if (exception is ReflectionTypeLoadException loadException && loadException.LoaderExceptions != null)
                {
                    if (includeStackTrace)
                        sb.AppendLine();
                    foreach (var e in loadException.LoaderExceptions)
                        FlattenInternal(e, sb, includeStackTrace);
                }

                exception = exception.InnerException;
                if (exception != null && includeStackTrace)
                    sb.AppendLine();
            }
        }

        private static class GenericCaster<T>
        {
            public static readonly Func<T, T> Cast = arg1 => arg1;
        }
    }
}