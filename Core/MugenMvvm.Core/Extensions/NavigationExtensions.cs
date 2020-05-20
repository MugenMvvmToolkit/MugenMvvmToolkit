using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static string GetUniqueNavigationId(this INavigationProvider navigationProvider, IMetadataOwner<IMetadataContext> target)
        {
            return null!; //todo review
        }

        public static void OnNavigating(this INavigationDispatcher dispatcher, INavigationContext context, Func<INavigationDispatcher, INavigationContext, bool> completeNavigationCallback,
            Action<INavigationDispatcher, INavigationContext, Exception?>? fallback = null, CancellationToken cancellationToken = default)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            Should.NotBeNull(context, nameof(context));
            dispatcher.OnNavigatingAsync(context, cancellationToken)
                .ContinueWith(task => InvokeCompletedCallback(task, context, dispatcher, completeNavigationCallback, fallback, cancellationToken), TaskContinuationOptions.ExecuteSynchronously);
        }

        public static INavigationContext GetNavigationContext(this INavigationDispatcher dispatcher, IMetadataOwner<IMetadataContext> target, INavigationProvider navigationProvider,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            return dispatcher.GetNavigationContext(target, navigationProvider, navigationProvider.GetUniqueNavigationId(target), navigationType, navigationMode, metadata);
        }

        public static INavigationContext GetNavigationContext(this INavigationDispatcher dispatcher, object target, INavigationProvider navigationProvider,
            string navigationId, NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            var navigationContext = dispatcher.GetNavigationContext(navigationProvider, navigationId, navigationType, navigationMode, metadata);
            navigationContext.Metadata.Set(NavigationMetadata.Target, target);
            return navigationContext;
        }

        public static Task WaitNavigationAsync(this INavigationDispatcher dispatcher, Func<INavigationCallback, bool> filter, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            Should.NotBeNull(filter, nameof(filter));
            var entries = dispatcher.GetNavigationEntries(metadata);
            LazyList<Task> tasks = default;
            for (var i = 0; i < entries.Count; i++)
            {
                var callbacks = dispatcher.GetNavigationCallbacks(entries[i], metadata);
                for (var j = 0; j < callbacks.Count; j++)
                {
                    var callback = callbacks[j];
                    if (filter(callback))
                        tasks.Add(callback.AsTask());
                }
            }

            if (tasks.List == null)
                return Default.CompletedTask;
            return Task.WhenAll(tasks.List);
        }

        public static Task<IReadOnlyMetadataContext> AsTask(this INavigationCallback callback)
        {
            Should.NotBeNull(callback, nameof(callback));
            var result = new NavigationCallbackTaskListener();
            callback.AddCallback(result);
            return result.Task;
        }

        private static void InvokeCompletedCallback(Task<bool> task, INavigationContext navigationContext,
            INavigationDispatcher dispatcher, Func<INavigationDispatcher, INavigationContext, bool> completeNavigationCallback,
            Action<INavigationDispatcher, INavigationContext, Exception?>? fallback, CancellationToken cancellationToken)
        {
            try
            {
                if (task.IsCanceled)
                {
                    fallback?.Invoke(dispatcher, navigationContext, null);
                    dispatcher.OnNavigationCanceled(navigationContext, cancellationToken);
                    return;
                }

                if (task.IsFaulted)
                {
                    fallback?.Invoke(dispatcher, navigationContext, task.Exception);
                    dispatcher.OnNavigationFailed(navigationContext, task.Exception);
                    return;
                }

                if (task.Result)
                {
                    if (completeNavigationCallback(dispatcher, navigationContext))
                        dispatcher.OnNavigated(navigationContext);
                }
                else
                {
                    fallback?.Invoke(dispatcher, navigationContext, null);
                    dispatcher.OnNavigationCanceled(navigationContext, cancellationToken);
                }
            }
            catch (Exception e)
            {
                fallback?.Invoke(dispatcher, navigationContext, e);
                dispatcher.OnNavigationFailed(navigationContext, e);
            }
        }

        #endregion
    }
}