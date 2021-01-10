using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MugenMvvm.Busy;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static Task WhenAll(this ItemOrListEditor<Task> editor)
        {
            if (editor.Count == 0)
                return Default.CompletedTask;
            if (editor.Count == 1)
                return editor[0];
            return Task.WhenAll((IList<Task>) editor.GetRawValue()!);
        }

        public static TTask WithBusyIndicator<TTask>(this TTask task, IHasService<IBusyManager> busyManager,
            object? message = null, int millisecondsDelay = 0, IReadOnlyMetadataContext? metadata = null)
            where TTask : Task
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            return task.WithBusyIndicator(busyManager.Service, message, millisecondsDelay, metadata);
        }

        public static TTask WithBusyIndicator<TTask>(this TTask task, IBusyManager busyManager, object? message = null, int millisecondsDelay = 0, IReadOnlyMetadataContext? metadata = null)
            where TTask : Task
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(busyManager, nameof(busyManager));
            if (task.IsCompleted)
                return task;
            if (millisecondsDelay == 0 && message is IHasBusyDelayMessage hasBusyDelay)
                millisecondsDelay = hasBusyDelay.Delay;
            var token = busyManager.BeginBusy(millisecondsDelay > 0 ? new DelayBusyRequest(message, millisecondsDelay) : message, metadata);
            task.ContinueWith((t, o) => ((IDisposable) o!).Dispose(), token, TaskContinuationOptions.ExecuteSynchronously);
            return task;
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

        public static void TrySetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, ValueTask<TResult> task, TaskContinuationOptions continuationOptions = TaskContinuationOptions.ExecuteSynchronously)
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

        public static void TrySetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, Task task, TaskContinuationOptions continuationOptions = TaskContinuationOptions.ExecuteSynchronously)
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

        #endregion
    }
}