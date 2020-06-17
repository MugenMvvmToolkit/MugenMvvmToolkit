using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Commands;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Internal;
using MugenMvvm.Validation.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static void SetErrors(this IValidator validator, object target, string memberName, ItemOrList<object, IReadOnlyList<object>> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(memberName, nameof(memberName));
            InlineValidatorComponent? component = null;
            var components = validator.GetComponents<InlineValidatorComponent>();
            for (int i = 0; i < components.Length; i++)
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

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, Action action, IReadOnlyMetadataContext? metadata = null)
        {
            threadDispatcher.DefaultIfNull().Execute<object?>(executionMode, action, null, metadata);
        }

        public static void Execute<TState>(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, in TState state, Action<TState> action, IReadOnlyMetadataContext? metadata = null)
        {
            threadDispatcher.DefaultIfNull().Execute(executionMode, action, state, metadata);
        }

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, IThreadDispatcherHandler handler, IReadOnlyMetadataContext? metadata = null)
        {
            threadDispatcher.DefaultIfNull().Execute<object?>(executionMode, handler, null, metadata);
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedValueProvider valueProvider, TItem item, string path, Func<TItem, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.GetOrAdd(item, path, valueFactory, (it, s) => s(it));
        }

        public static ICompositeCommand GetCommand(this ICommandProvider? mediatorProvider, Action execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<object>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand<T>(this ICommandProvider? mediatorProvider, Action<T> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<T>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand(this ICommandProvider? mediatorProvider, Func<Task> execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<object>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand<T>(this ICommandProvider? mediatorProvider, Func<T, Task> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<T>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand<TRequest>(this ICommandProvider commandProvider, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(commandProvider, nameof(commandProvider));
            var result = commandProvider.TryGetCommand(request, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<ICommandProviderComponent>(commandProvider);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BusyMessageHandlerType value, BusyMessageHandlerType flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BindingFlags value, BindingFlags flag)
        {
            return (value & flag) == flag;
        }

        public static TTo CastGeneric<TFrom, TTo>(in TFrom value)
        {
            if (typeof(TFrom) == typeof(TTo))
                return ((FuncIn<TFrom, TTo>)(object)GenericCaster<TFrom>.Cast).Invoke(value);
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
        public static string Format(this string format, params object?[] args)
        {
            return string.Format(format, args);
        }

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
                        tcs.TrySetException(task.Exception.InnerExceptions);
                        break;
                    case TaskStatus.RanToCompletion:
                        var t = task as Task<TResult>;
                        tcs.TrySetResult(t == null ? default! : t.Result);
                        break;
                }
            }
            else
                task.ContinueWith((t, o) => ((TaskCompletionSource<TResult>)o).TrySetFromTask(t), tcs, continuationOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Execute<TState>(this IThreadDispatcherHandler<TState> handler, object state)
        {
            handler.Execute((TState)state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Invoke<TState>(this Action<TState> handler, object state)
        {
            handler.Invoke((TState)state);
        }

        internal static void ReleaseWeakReference(this IValueHolder<IWeakReference>? valueHolder)
        {
            valueHolder?.Value?.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IWeakReference ToWeakReference(this object? item)
        {
            return MugenService.WeakReferenceProvider.GetWeakReference(item);
        }

        internal static void TrySetExceptionEx<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            if (e is AggregateException aggregateException)
                tcs.TrySetException(aggregateException.InnerExceptions);
            else
                tcs.SetException(e);
        }

        internal static bool LazyInitialize<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class
        {
            return Interlocked.CompareExchange(ref item, value, null) == null;
        }

        internal static bool LazyInitializeDisposable<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class, IDisposable
        {
            if (!LazyInitialize(ref item, value))
            {
                value.Dispose();
                return false;
            }

            return true;
        }

        private static ICompositeCommand GetCommandInternal<T>(ICommandProvider? mediatorProvider, Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionMode? executionMode,
            ThreadExecutionMode? eventThreadMode, IReadOnlyList<object>? notifiers, Func<object, bool>? canNotify, IReadOnlyMetadataContext? metadata)
        {
            var request = new DelegateCommandRequest((in DelegateCommandRequest r, DelegateCommandRequest.IProvider provider, IReadOnlyMetadataContext? m) => provider.TryGetCommand<T>(r, m),
                execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify);
            return mediatorProvider.DefaultIfNull().GetCommand(request, metadata);
        }

        #endregion

        #region Nested types

        private static class GenericCaster<T>
        {
            #region Fields

            public static readonly FuncIn<T, T> Cast = (in T arg1) => arg1;

            #endregion
        }

        #endregion
    }
}