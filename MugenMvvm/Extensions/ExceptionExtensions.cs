using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static Task<T> TaskFromException<T>(this Exception exception)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.TrySetExceptionEx(exception);
            return tcs.Task;
        }

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

        internal static void TrySetExceptionEx<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            if (e.TryGetCanceledException(out var canceledException))
                tcs.TrySetCanceled(canceledException.CancellationToken);
            else if (e is AggregateException aggregateException)
                tcs.TrySetException(aggregateException.InnerExceptions);
            else
                tcs.SetException(e);
        }

        internal static bool TryGetCanceledException(this Exception e, [NotNullWhen(true)] out OperationCanceledException? canceledException)
        {
            if (e is OperationCanceledException canceled)
            {
                canceledException = canceled;
                return true;
            }

            if (e is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1 && aggregateException.InnerExceptions[0] is OperationCanceledException c)
            {
                canceledException = c;
                return true;
            }

            canceledException = null;
            return false;
        }

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
                    for (var index = 0; index < loadException.LoaderExceptions.Length; index++)
                        FlattenInternal(loadException.LoaderExceptions[index], sb, includeStackTrace);
                }

                exception = exception.InnerException;
                if (exception != null && includeStackTrace)
                    sb.AppendLine();
            }
        }

        #endregion
    }
}