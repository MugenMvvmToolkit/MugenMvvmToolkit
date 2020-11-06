using System;
using System.Linq;
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

        public static object? GetNextNavigationTarget(this INavigationDispatcher navigationDispatcher, INavigationContext navigationContext) =>
            navigationDispatcher.GetTopNavigation(navigationContext, (entry, context, m) =>
            {
                if (entry.NavigationType == context.NavigationType && entry.Target != null && !Equals(entry.Target, context.Target))
                    return entry.Target;
                return null;
            }) ?? navigationDispatcher.GetTopNavigation(navigationContext, (entry, context, m) =>
            {
                if (entry.Target != null && !Equals(entry.Target, context.Target))
                    return entry.Target;
                return null;
            });

        public static Task ClearBackStackAsync(this INavigationDispatcher navigationDispatcher, NavigationType navigationType, object navigationTarget,
            bool includePending = false, IReadOnlyMetadataContext? metadata = null, IPresenter? presenter = null)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(navigationTarget, nameof(navigationTarget));
            IReadOnlyMetadataContext? closeMetadata = null;
            var callbacks = ItemOrListEditor.Get<Task>();
            foreach (var navigationEntry in navigationDispatcher.GetNavigationEntries(metadata))
            {
                if (!includePending && navigationEntry.IsPending)
                    continue;

                if (navigationEntry.NavigationType != navigationType || navigationEntry.Target == null || Equals(navigationEntry.Target, navigationTarget))
                    continue;

                closeMetadata ??= new MetadataContext(metadata) {{NavigationMetadata.ForceClose, true}, {NavigationMetadata.NavigationType, navigationType}};
                foreach (var result in presenter.DefaultIfNull().TryClose(navigationEntry.Target, default, closeMetadata))
                {
                    foreach (var navigationCallback in navigationDispatcher.GetNavigationCallbacks(result, metadata))
                    {
                        if (navigationCallback.CallbackType == NavigationCallbackType.Closing)
                            callbacks.Add(navigationCallback.ToTask(false));
                    }
                }
            }

            return callbacks.WhenAll();
        }

        public static TView? GetTopView<TView>(this INavigationDispatcher navigationDispatcher, NavigationType? navigationType = null, bool includePending = true, IReadOnlyMetadataContext? metadata = null)
            where TView : class =>
            navigationDispatcher.GetTopNavigation((navigationType, includePending), (entry, state, m) =>
            {
                if (!state.includePending && entry.IsPending)
                    return null;
                if (state.navigationType != null && entry.NavigationType != state.navigationType || !(entry.Target is IViewModelBase viewModel))
                    return null;
                foreach (var t in MugenService.ViewManager.GetViews(viewModel, m))
                {
                    if (t.Target is TView view)
                        return view;
                }

                return null;
            }, metadata);

        public static T? GetTopNavigationTarget<T>(this INavigationDispatcher navigationDispatcher, NavigationType? navigationType = null, bool includePending = true, IReadOnlyMetadataContext? metadata = null)
            where T : class =>
            navigationDispatcher.GetTopNavigation((navigationType, includePending), (entry, state, m) =>
            {
                if (!state.includePending && entry.IsPending)
                    return null;
                if ((state.navigationType == null || entry.NavigationType == state.navigationType) && entry.Target is T target)
                    return target;
                return null;
            }, metadata);

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
            foreach (var navigationEntry in list.OrderByDescending(entry => entry.GetOrDefault(NavigationMetadata.NavigationDate)))
            {
                var result = predicate(navigationEntry, state, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IPresenterResult GetPresenterResult(this INavigationProvider navigationProvider, IViewModelBase viewModel, NavigationType navigationType, IReadOnlyMetadataContext? metadata = null) =>
            navigationProvider.GetPresenterResult(viewModel, navigationProvider.GetNavigationId(viewModel), navigationType, metadata);

        public static IPresenterResult GetPresenterResult(this INavigationProvider navigationProvider, object? target, string navigationId, NavigationType navigationType, IReadOnlyMetadataContext? metadata = null) =>
            new PresenterResult(target, navigationId, navigationProvider, navigationType, metadata);

        public static string GetNavigationId(this INavigationProvider navigationProvider, IViewModelBase viewModel)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(viewModel, nameof(viewModel));
            return $"{navigationProvider.Id}/{viewModel.GetId()}";
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

        public static Task WaitNavigationAsync<TState>(this INavigationDispatcher dispatcher, object? navigationTarget, TState state,
            Func<INavigationCallback, TState, bool> filter, bool includePending = true, bool isSerializable = false, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            Should.NotBeNull(filter, nameof(filter));
            var tasks = ItemOrListEditor.Get<Task>();
            foreach (var t in dispatcher.GetNavigationEntries(metadata))
            {
                if (!includePending && t.IsPending)
                    continue;

                if (navigationTarget != null && Equals(t.Target, navigationTarget))
                    continue;

                foreach (var callback in dispatcher.GetNavigationCallbacks(t, metadata))
                {
                    if (filter(callback, state))
                        tasks.Add(callback.ToTask(isSerializable));
                }
            }

            return tasks.WhenAll();
        }

        public static Task<INavigationContext> ToTask(this INavigationCallback callback, bool isSerializable)
        {
            Should.NotBeNull(callback, nameof(callback));
            var result = new NavigationCallbackTaskListener(isSerializable);
            callback.AddCallback(result);
            return result.Task;
        }

        #endregion
    }
}