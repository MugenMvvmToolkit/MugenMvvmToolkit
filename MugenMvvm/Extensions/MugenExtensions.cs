using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Commands;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;
using MugenMvvm.Validation.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IViewModelBase GetViewModel(this IViewModelManager viewModelManager, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            Should.NotBeNull(request, nameof(request));
            var viewModel = viewModelManager.TryGetViewModel(request, metadata);
            if (viewModel == null)
                ExceptionManager.ThrowRequestNotSupported<IEntityTrackingCollectionProviderComponent>(viewModelManager, request, metadata);
            return viewModel;
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

        public static void Serialize(this ISerializer serializer, Stream stream, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            if (!serializer.TrySerialize(stream, request, metadata))
                ExceptionManager.ThrowRequestNotSupported<ISerializerComponent>(serializer, request, metadata);
        }

        public static object? Deserialize(this ISerializer serializer, Stream stream, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            if (!serializer.TryDeserialize(stream, metadata, out var result))
                ExceptionManager.ThrowRequestNotSupported<ISerializerComponent>(serializer, stream, metadata);
            return result;
        }

        public static IValidator GetValidator(this IValidationManager validatorProvider, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            var result = validatorProvider.TryGetValidator(request, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IValidatorProviderComponent>(validatorProvider, request, metadata);
            return result;
        }

        public static ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> Show(this IPresenter presenter, object request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            var result = presenter.TryShow(request, cancellationToken, metadata);
            if (result.IsNullOrEmpty())
                ExceptionManager.ThrowPresenterCannotShowRequest(request, metadata);
            return result;
        }

        public static IWeakReference GetWeakReference(this IWeakReferenceManager weakReferenceManager, object? item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(weakReferenceManager, nameof(weakReferenceManager));
            var result = weakReferenceManager.TryGetWeakReference(item, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IWeakReferenceProviderComponent>(weakReferenceManager, item, metadata);
            return result;
        }

        public static IComponentCollection GetComponentCollection(this IComponentCollectionManager provider, object owner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(provider, nameof(provider));
            Should.NotBeNull(owner, nameof(owner));
            var collection = provider.TryGetComponentCollection(owner, metadata);
            if (collection == null)
                ExceptionManager.ThrowRequestNotSupported<IComponentCollectionProviderComponent>(provider, owner, metadata);
            return collection;
        }

        public static void SetErrors(this IValidator validator, object target, string memberName, ItemOrList<object, IReadOnlyList<object>> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(memberName, nameof(memberName));
            InlineValidatorComponent? component = null;
            var components = validator.GetComponents<InlineValidatorComponent>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].Target == target)
                {
                    component = components[i];
                    break;
                }
            }

            if (component == null)
            {
                component = new InlineValidatorComponent(target);
                validator.AddComponent(component);
            }

            component.SetErrors(memberName, errors, metadata);
        }

        public static void ExecuteRaw(IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (!threadDispatcher.DefaultIfNull().TryExecute(executionMode, handler, state, metadata))
                ExceptionManager.ThrowRequestNotSupported<IThreadDispatcherComponent>(threadDispatcher.DefaultIfNull(), handler, metadata);
        }

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, Action action, IReadOnlyMetadataContext? metadata = null) =>
            ExecuteRaw(threadDispatcher, executionMode, action, null, metadata);

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, Action<object?> action, object? state, IReadOnlyMetadataContext? metadata = null) =>
            ExecuteRaw(threadDispatcher, executionMode, action, state, metadata);

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, IThreadDispatcherHandler handler, object? state, IReadOnlyMetadataContext? metadata = null) =>
            ExecuteRaw(threadDispatcher, executionMode, handler, state, metadata);

        public static IViewModelBase? TryGetViewModelView<TView>(object request, out TView? view) where TView : class
        {
            if (request is ViewModelViewRequest viewModelViewRequest)
            {
                view = viewModelViewRequest.View as TView;
                return viewModelViewRequest.ViewModel;
            }

            if (request is IViewModelBase vm)
            {
                view = null;
                return vm;
            }

            if (request is IHasTarget<object?> hasTarget && hasTarget.Target is IViewModelBase result)
            {
                view = null;
                return result;
            }

            view = request as TView;
            return null;
        }

        public static ICompositeCommand GetCommand(this ICommandManager? commandManager, Action execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull().GetCommand<object>(DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<T>(this ICommandManager? commandManager, Action<T> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull().GetCommand<T>(DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand(this ICommandManager? commandManager, Func<Task> execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull().GetCommand<object>(DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<T>(this ICommandManager? commandManager, Func<T, Task> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull().GetCommand<T>(DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<TParameter>(this ICommandManager commandManager, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(commandManager, nameof(commandManager));
            var result = commandManager.TryGetCommand<TParameter>(request, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<ICommandProviderComponent>(commandManager, request, metadata);
            return result;
        }

        public static bool IsInBackground(this IMugenApplication application, bool defaultValue = false)
        {
            Should.NotBeNull(application, nameof(application));
            return application.GetMetadataOrDefault().Get(ApplicationMetadata.IsInBackground, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BusyMessageHandlerType value, BusyMessageHandlerType flag) => (value & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BindingFlags value, BindingFlags flag) => (value & flag) == flag;

        public static TTo CastGeneric<TFrom, TTo>(TFrom value)
        {
            if (typeof(TFrom) == typeof(TTo))
                return ((Func<TFrom, TTo>)(object)GenericCaster<TFrom>.Cast).Invoke(value);
            return (TTo)(object)value!;
        }

        public static bool MemberNameEqual(string changedMember, string listenedMember, bool emptyListenedMemberResult = false)
        {
            if (string.Equals(changedMember, listenedMember) || string.IsNullOrEmpty(changedMember))
                return true;
            if (string.IsNullOrEmpty(listenedMember))
                return emptyListenedMemberResult;

            if (listenedMember[0] == '[')
            {
                if (Default.IndexerName.Equals(changedMember))
                    return true;
                if (changedMember.StartsWith("Item[", StringComparison.Ordinal))
                {
                    int i = 4, j = 0;
                    while (i < changedMember.Length)
                    {
                        if (j >= listenedMember.Length)
                            return false;
                        var c1 = changedMember[i];
                        var c2 = listenedMember[j];
                        if (c1 == c2)
                        {
                            ++i;
                            ++j;
                        }
                        else if (c1 == '"')
                            ++i;
                        else if (c2 == '"')
                            ++j;
                        else
                            return false;
                    }

                    return j == listenedMember.Length;
                }
            }

            return false;
        }

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args) => string.Format(format, args);

        public static void TrySetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, Task task, TaskContinuationOptions continuationOptions = TaskContinuationOptions.ExecuteSynchronously)
        {
            Should.NotBeNull(tcs, nameof(tcs));
            Should.NotBeNull(task, nameof(task));
            if (task.IsCompleted)
            {
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        tcs.TrySetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        tcs.TrySetException(task.Exception!.InnerExceptions);
                        break;
                    case TaskStatus.RanToCompletion:
                        var t = task as Task<TResult>;
                        tcs.TrySetResult(t == null ? default! : t.Result);
                        break;
                }
            }
            else
                task.ContinueWith((t, o) => ((TaskCompletionSource<TResult>)o!).TrySetFromTask(t), tcs, continuationOptions);
        }

        internal static void ReleaseWeakReference(this IValueHolder<IWeakReference>? valueHolder) => valueHolder?.Value?.Release();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IWeakReference ToWeakReference(this object? item) => MugenService.WeakReferenceManager.GetWeakReference(item);

        internal static Task ContinueWithEx<TState>(this Task task, TState state, Action<Task, TState> execute)
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(execute, nameof(execute));
            if (task.IsCompleted)
            {
                try
                {
                    execute(task, state);
                    return task;
                }
                catch (Exception e)
                {
                    return Task.FromException(e);
                }
            }

            return task.ContinueWith((t, o) =>
            {
                var tuple = (Tuple<TState, Action<Task, TState>>)o!;
                tuple.Item2(t, tuple.Item1);
            }, Tuple.Create(state, execute), TaskContinuationOptions.ExecuteSynchronously);
        }

        internal static Task ContinueWithEx<T, TState>(this Task<T> task, TState state, Action<Task<T>, TState> execute)
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(execute, nameof(execute));
            if (task.IsCompleted)
            {
                try
                {
                    execute(task, state);
                    return task;
                }
                catch (Exception e)
                {
                    return Task.FromException(e);
                }
            }

            return task.ContinueWith((t, o) =>
            {
                var tuple = (Tuple<TState, Action<Task<T>, TState>>)o!;
                tuple.Item2(t, tuple.Item1);
            }, Tuple.Create(state, execute), TaskContinuationOptions.ExecuteSynchronously);
        }

        internal static void TrySetExceptionEx<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            if (e is AggregateException aggregateException)
                tcs.TrySetException(aggregateException.InnerExceptions);
            else
                tcs.SetException(e);
        }

        internal static bool LazyInitialize<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class => Interlocked.CompareExchange(ref item, value, null) == null;

        internal static bool LazyInitializeDisposable<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class, IDisposable
        {
            if (!LazyInitialize(ref item, value))
            {
                value.Dispose();
                return false;
            }

            return true;
        }

#if SPAN_API
        //https://github.com/dotnet/runtime/pull/295
        internal static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, char separator)
            => new SpanSplitEnumerator<char>(span, separator);
#endif

        #endregion

        #region Nested types

#if SPAN_API
        public ref struct SpanSplitEnumerator<T> where T : IEquatable<T>
        {
            private readonly ReadOnlySpan<T> _buffer;

            private readonly ReadOnlySpan<T> _separators;
            private readonly T _separator;

            private readonly int _separatorLength;
            private readonly bool _splitOnSingleToken;

            private readonly bool _isInitialized;

            private int _startCurrent;
            private int _endCurrent;
            private int _startNext;

            public SpanSplitEnumerator<T> GetEnumerator() => this;

            public Range Current => new Range(_startCurrent, _endCurrent);

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, ReadOnlySpan<T> separators)
            {
                _isInitialized = true;
                _buffer = span;
                _separators = separators;
                _separator = default!;
                _splitOnSingleToken = false;
                _separatorLength = _separators.Length != 0 ? _separators.Length : 1;
                _startCurrent = 0;
                _endCurrent = 0;
                _startNext = 0;
            }

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, T separator)
            {
                _isInitialized = true;
                _buffer = span;
                _separator = separator;
                _separators = default;
                _splitOnSingleToken = true;
                _separatorLength = 1;
                _startCurrent = 0;
                _endCurrent = 0;
                _startNext = 0;
            }

            public bool MoveNext()
            {
                if (!_isInitialized || _startNext > _buffer.Length)
                    return false;

                var slice = _buffer.Slice(_startNext);
                _startCurrent = _startNext;

                var separatorIndex = _splitOnSingleToken ? slice.IndexOf(_separator) : slice.IndexOf(_separators);
                var elementLength = separatorIndex != -1 ? separatorIndex : slice.Length;

                _endCurrent = _startCurrent + elementLength;
                _startNext = _endCurrent + _separatorLength;
                return true;
            }
        }
#endif

        private static class GenericCaster<T>
        {
            #region Fields

            public static readonly Func<T, T> Cast = arg1 => arg1;

            #endregion
        }

        #endregion
    }
}