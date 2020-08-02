using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Presenters;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static TView? GetTopView<TView>(this INavigationDispatcher navigationDispatcher, NavigationType? navigationType = null, IReadOnlyMetadataContext? metadata = null)
            where TView : class
        {
            return navigationDispatcher.GetTopNavigation(navigationType, (entry, type, m) =>
            {
                if (type != null && entry.NavigationType != type || !(entry.Target is IViewModelBase viewModel))
                    return null;
                foreach (var t in MugenService.ViewManager.GetViews(viewModel, m).Iterator())
                {
                    if (t.Target is TView view)
                        return view;
                }

                return null;
            }, metadata);
        }

        public static T? GetTopNavigationTarget<T>(this INavigationDispatcher navigationDispatcher, NavigationType? navigationType = null, IReadOnlyMetadataContext? metadata = null)
            where T : class
        {
            return navigationDispatcher.GetTopNavigation(navigationType, (entry, type, m) =>
            {
                if ((type == null || entry.NavigationType == type) && entry.Target is T target)
                    return target;
                return null;
            }, metadata);
        }

        public static TResult? GetTopNavigation<TResult, TState>(this INavigationDispatcher navigationDispatcher, TState state, Func<INavigationEntry, TState, IReadOnlyMetadataContext?, TResult?> predicate,
            IReadOnlyMetadataContext? metadata = null)
            where TResult : class
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(predicate, nameof(predicate));
            var entries = navigationDispatcher.GetNavigationEntries(metadata);
            if (entries.Item != null)
                return predicate(entries.Item, state, metadata);
            var list = entries.List;
            if (list == null)
                return null;
            foreach (var navigationEntry in list.OrderByDescending(entry => entry.GetMetadataOrDefault().Get(NavigationMetadata.NavigationDate)))
            {
                var result = predicate(navigationEntry, state, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IPresenterResult GetPresenterResult(this INavigationProvider navigationProvider, IViewModelBase viewModel, NavigationType navigationType, IReadOnlyMetadataContext? metadata = null)
        {
            return navigationProvider.GetPresenterResult(viewModel, navigationProvider.GetNavigationId(viewModel), navigationType, metadata);
        }

        public static IPresenterResult GetPresenterResult(this INavigationProvider navigationProvider, object? target, string navigationId, NavigationType navigationType, IReadOnlyMetadataContext? metadata = null)
        {
            return new PresenterResult(target, navigationId, navigationProvider, navigationType, metadata);
        }

        public static string GetNavigationId(this INavigationProvider navigationProvider, IViewModelBase viewModel)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(viewModel, nameof(viewModel));
            return $"{navigationProvider.Id}/{viewModel.Metadata.Get(ViewModelMetadata.Id):N}";
        }

        public static void OnNavigatingAsync<TState>(this INavigationDispatcher dispatcher, INavigationContext context, TState state,
            Func<INavigationDispatcher, INavigationContext, TState, bool> completeNavigationCallback,
            Action<INavigationDispatcher, INavigationContext, Exception?, TState>? fallback = null, CancellationToken cancellationToken = default)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            Should.NotBeNull(context, nameof(context));
            dispatcher.OnNavigatingAsync(context, cancellationToken).ContinueWith((task, st) =>
            {
                var tuple =
                    (Tuple<INavigationContext, INavigationDispatcher, Func<INavigationDispatcher, INavigationContext, TState, bool>, Action<INavigationDispatcher, INavigationContext, Exception?, TState>?,
                        CancellationToken, TState>)st!;
                InvokeCompletedCallback(task, tuple.Item1, tuple.Item6, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
            }, Tuple.Create(context, dispatcher, completeNavigationCallback, fallback, cancellationToken, state), TaskContinuationOptions.ExecuteSynchronously);
        }

        public static INavigationContext GetNavigationContext(this INavigationDispatcher dispatcher, object? target, INavigationProvider navigationProvider, string navigationId, NavigationType navigationType,
            NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            var result = dispatcher.TryGetNavigationContext(target, navigationProvider, navigationId, navigationType, navigationMode, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<INavigationContextProviderComponent>(dispatcher, target, metadata);
            return result;
        }

        public static INavigationContext GetNavigationContext(this INavigationDispatcher dispatcher, IViewModelBase target, INavigationProvider navigationProvider,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            return dispatcher.GetNavigationContext(target, navigationProvider, navigationProvider.GetNavigationId(target), navigationType, navigationMode, metadata);
        }

        public static Task WaitNavigationAsync<TState>(this INavigationDispatcher dispatcher, TState state, Func<INavigationCallback, TState, bool> filter, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            Should.NotBeNull(filter, nameof(filter));
            ItemOrListEditor<Task, List<Task>> tasks = ItemOrListEditor.Get<Task>();
            foreach (var t in dispatcher.GetNavigationEntries(metadata).Iterator())
            {
                foreach (var callback in dispatcher.GetNavigationCallbacks(t, metadata).Iterator())
                {
                    if (filter(callback, state))
                        tasks.Add(callback.AsTask());
                }
            }

            return tasks.ToItemOrList().WhenAll();
        }

        public static Task<INavigationContext> AsTask(this INavigationCallback callback)
        {
            Should.NotBeNull(callback, nameof(callback));
            var result = new NavigationCallbackTaskListener();
            callback.AddCallback(result);
            return result.Task;
        }

        private static void InvokeCompletedCallback<TState>(Task<bool> task, INavigationContext navigationContext, TState state,
            INavigationDispatcher dispatcher, Func<INavigationDispatcher, INavigationContext, TState, bool> completeNavigationCallback,
            Action<INavigationDispatcher, INavigationContext, Exception?, TState>? fallback, CancellationToken cancellationToken)
        {
            try
            {
                if (task.IsCanceled || cancellationToken.IsCancellationRequested)
                {
                    fallback?.Invoke(dispatcher, navigationContext, null, state);
                    dispatcher.OnNavigationCanceled(navigationContext, cancellationToken);
                    return;
                }

                if (task.IsFaulted)
                {
                    fallback?.Invoke(dispatcher, navigationContext, task.Exception, state);
                    dispatcher.OnNavigationFailed(navigationContext, task.Exception!);
                    return;
                }

                if (task.Result)
                {
                    if (completeNavigationCallback(dispatcher, navigationContext, state))
                        dispatcher.OnNavigated(navigationContext);
                }
                else
                {
                    fallback?.Invoke(dispatcher, navigationContext, null, state);
                    dispatcher.OnNavigationCanceled(navigationContext, cancellationToken);
                }
            }
            catch (Exception e)
            {
                fallback?.Invoke(dispatcher, navigationContext, e, state);
                dispatcher.OnNavigationFailed(navigationContext, e);
            }
        }

        #endregion
    }
}