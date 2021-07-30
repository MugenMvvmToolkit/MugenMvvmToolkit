﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MugenMvvm.Busy;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCompletedSuccessfully(this Task task)
        {
#if NET461
            return task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
#else
            return task.IsCompletedSuccessfully;
#endif
        }

        public static T GetResult<T>(this ValueTask<T> task)
        {
            if (task.IsCompletedSuccessfully)
                return task.Result;
            return task.AsTask().Result;
        }

        public static async ValueTask<T> LogException<T>(this ValueTask<T> task, UnhandledExceptionType exceptionType)
        {
            if (task.IsCompletedSuccessfully)
                return task.Result;
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                MugenService.Application.OnUnhandledException(e, exceptionType);
                throw;
            }
        }

        public static T LogException<T>(this T task, UnhandledExceptionType exceptionType) where T : Task
        {
            Should.NotBeNull(task, nameof(task));
            task.ContinueWith((t, s) => MugenService.Application.OnUnhandledException(t.Exception!, (UnhandledExceptionType) s!), exceptionType,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static Task WhenAll(this ItemOrListEditor<Task> editor)
        {
            if (editor.Count == 0)
                return Task.CompletedTask;
            if (editor.Count == 1)
                return editor[0];
            return Task.WhenAll(editor.AsList());
        }

        public static async Task<bool> WhenAll(this ItemOrListEditor<INavigationCallback> callbacks, bool throwOnCancel, bool throwOnDispose, bool isSerializable)
        {
            var result = false;
            foreach (var callback in callbacks)
            {
                if (callback == null)
                    continue;
                try
                {
                    if (await callback.AsTask(isSerializable).ConfigureAwait(false) != null)
                        result = true;
                }
                catch (OperationCanceledException)
                {
                    if (throwOnCancel)
                        throw;
                }
                catch (ObjectDisposedException)
                {
                    if (throwOnDispose)
                        throw;
                }
            }

            return result;
        }

        public static TTask WithBusyIndicator<TTask>(this TTask task, IHasService<IBusyManager> busyManager,
            object? message = null, int millisecondsDelay = 0, IReadOnlyMetadataContext? metadata = null)
            where TTask : Task
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            return task.WithBusyIndicator(busyManager.GetService(false)!, message, millisecondsDelay, metadata);
        }

        public static TTask WithBusyIndicator<TTask>(this TTask task, IBusyManager busyManager, object? message = null, int millisecondsDelay = 0,
            IReadOnlyMetadataContext? metadata = null)
            where TTask : Task
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(busyManager, nameof(busyManager));
            if (task.IsCompleted)
                return task;
            if (millisecondsDelay == 0 && message is IHasDelayBusyMessage hasBusyDelay)
                millisecondsDelay = hasBusyDelay.Delay;
            var token = busyManager.BeginBusy(millisecondsDelay > 0 ? new DelayBusyRequest(message, millisecondsDelay) : message, metadata);
            task.ContinueWith((t, o) => ((IDisposable) o!).Dispose(), token, TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static ValueTask<T> WithBusyIndicator<T>(this ValueTask<T> task, IHasService<IBusyManager> busyManager, object? message = null, int millisecondsDelay = 0,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            return task.WithBusyIndicator(busyManager.GetService(false)!, message, millisecondsDelay, metadata);
        }

        public static async ValueTask<T> WithBusyIndicator<T>(this ValueTask<T> task, IBusyManager busyManager, object? message = null, int millisecondsDelay = 0,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            if (task.IsCompleted)
                return task.Result;

            if (millisecondsDelay == 0 && message is IHasDelayBusyMessage hasBusyDelay)
                millisecondsDelay = hasBusyDelay.Delay;
            using var token = busyManager.BeginBusy(millisecondsDelay > 0 ? new DelayBusyRequest(message, millisecondsDelay) : message, metadata);
            return await task.ConfigureAwait(false);
        }

        public static Task ToTask(this Exception exception) => ToTask<object?>(exception);

        public static Task<T> ToTask<T>(this Exception exception)
        {
            Should.NotBeNull(exception, nameof(exception));
            exception = exception.TryGetBaseException(out var canceledException);
            if (canceledException == null)
                return Task.FromException<T>(exception);
            return Task.FromCanceled<T>(canceledException.CancellationToken);
        }

        public static void TrySetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, ValueTask<TResult> task,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.ExecuteSynchronously)
        {
            Should.NotBeNull(tcs, nameof(tcs));
            if (task.IsCompleted)
            {
                try
                {
                    tcs.TrySetResult(task.Result);
                }
                catch (Exception e)
                {
                    tcs.TrySetExceptionEx(e);
                }
            }
            else
                tcs.TrySetFromTask(task.AsTask(), continuationOptions);
        }

        public static void TrySetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, Task task,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.ExecuteSynchronously)
        {
            Should.NotBeNull(tcs, nameof(tcs));
            Should.NotBeNull(task, nameof(task));
            if (task.IsCompleted)
            {
                try
                {
                    if (task is Task<TResult> t)
                        tcs.TrySetResult(t.Result);
                    else
                    {
                        task.Wait();
                        tcs.TrySetResult(default!);
                    }
                }
                catch (Exception e)
                {
                    tcs.TrySetExceptionEx(e);
                }
            }
            else
                task.ContinueWith((t, o) => ((TaskCompletionSource<TResult>) o!).TrySetFromTask(t), tcs, continuationOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<T> AsValueTask<T>(this Task<T>? task)
        {
            if (task == null)
                return default;
            return new ValueTask<T>(task);
        }

        internal static void TrySetExceptionEx<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            e = e.TryGetBaseException(out var canceledException);
            if (canceledException == null)
                tcs.TrySetException(e);
            else
                tcs.TrySetCanceled(canceledException.CancellationToken);
        }

        internal static Exception TryGetBaseException(this Exception e, out OperationCanceledException? canceledException)
        {
            while (true)
            {
                if (e is OperationCanceledException canceled)
                {
                    canceledException = canceled;
                    return e;
                }

                if (e is AggregateException aggregateException)
                {
                    e = aggregateException.GetBaseException();
                    continue;
                }

                canceledException = null;
                return e;
            }
        }
    }
}