using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IPresenterResult GetPresenterResult(this INavigationProvider provider, NavigationType navigationType, IViewModelBase viewModel,
            IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            return GetPresenterResult(provider, provider.GetUniqueNavigationOperationId(viewModel), navigationType, viewModel, metadata, metadataContextProvider);
        }

        public static IPresenterResult GetPresenterResult(this INavigationProvider provider, string navigationOperationId, NavigationType navigationType, IViewModelBase viewModel,
            IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            var result = new PresenterResult(provider, navigationOperationId, navigationType, metadataContextProvider, metadata);
            result.Metadata.Set(NavigationMetadata.ViewModel, viewModel);
            return result;
        }

        public static string GetUniqueNavigationOperationId(this INavigationProvider navigationProvider, IViewModelBase viewModel)
        {
            return null!;//todo review
        }

        public static INavigationContext GetNavigationContext(this INavigationDispatcher dispatcher, IViewModelBase viewModel, INavigationProvider navigationProvider, string navigationOperationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var navigationContext = dispatcher.GetNavigationContext(navigationProvider, navigationOperationId, navigationType, navigationMode, metadata);
            navigationContext.Metadata.Set(NavigationMetadata.ViewModel, viewModel);
            return navigationContext;
        }

        public static INavigationContext GetNavigationContext(this INavigationDispatcher dispatcher, IViewModelBase viewModel, INavigationProvider navigationProvider,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var navigationContext = dispatcher
                .GetNavigationContext(navigationProvider, navigationProvider.GetUniqueNavigationOperationId(viewModel), navigationType, navigationMode, metadata);
            navigationContext.Metadata.Set(NavigationMetadata.ViewModel, viewModel);
            return navigationContext;
        }

        public static void OnNavigating(this INavigationDispatcher dispatcher, INavigationContext context, Func<INavigationDispatcher, INavigationContext, bool> completeNavigationCallback,
            Action<INavigationDispatcher, INavigationContext, Exception?>? fallback = null)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            Should.NotBeNull(context, nameof(context));
            dispatcher.OnNavigatingAsync(context)
                .ContinueWith(task => InvokeCompletedCallback(task, context, dispatcher, completeNavigationCallback, fallback), TaskContinuationOptions.ExecuteSynchronously);
        }

        public static INavigationContext GetNavigationContext(this INavigationDispatcher dispatcher, INavigationProvider navigationProvider, string navigationOperationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            var components = dispatcher.GetComponents<INavigationContextProviderComponent>(metadata);
            for (int i = 0; i < components.Length; i++)
            {
                var context = components[i].TryGetNavigationContext(navigationProvider, navigationOperationId, navigationType, navigationMode, metadata);
                if (context != null)
                    return context;
            }

            ExceptionManager.ThrowObjectNotInitialized(dispatcher, components);
            return null;
        }

        public static IReadOnlyList<INavigationCallback> GetCallbacks(this INavigationDispatcher dispatcher, INavigationEntry entry, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            var components = dispatcher.GetComponents<INavigationCallbackProviderComponent>(metadata);
            List<INavigationCallback>? result = null;
            for (int i = 0; i < components.Length; i++)
            {
                var callbacks = components[i].TryGetCallbacks(entry, metadata);
                if (callbacks == null || callbacks.Count == 0)
                    continue;
                if (result == null)
                    result = new List<INavigationCallback>();
                result.AddRange(callbacks);
            }

            return result ?? (IReadOnlyList<INavigationCallback>)Default.EmptyArray<INavigationCallback>();
        }

        public static INavigationEntry? GetPreviousNavigationEntry(this INavigationDispatcher dispatcher, INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            var components = dispatcher.GetComponents<INavigationEntryFinderComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetPreviousNavigationEntry(navigationEntry, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static Task WaitNavigationAsync(this INavigationDispatcher dispatcher, Func<INavigationCallback, bool> filter, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            Should.NotBeNull(filter, nameof(filter));
            var entries = dispatcher.GetNavigationEntries(null, metadata);
            List<Task>? tasks = null;
            for (var i = 0; i < entries.Count; i++)
            {
                var callbacks = dispatcher.GetCallbacks(entries[i], metadata);
                for (var j = 0; j < callbacks.Count; j++)
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

        private static void InvokeCompletedCallback(Task<bool> task, INavigationContext navigationContext,
            INavigationDispatcher dispatcher, Func<INavigationDispatcher, INavigationContext, bool> completeNavigationCallback,
            Action<INavigationDispatcher, INavigationContext, Exception?>? fallback)
        {
            try
            {
                if (task.IsCanceled)
                {
                    fallback?.Invoke(dispatcher, navigationContext, null);
                    dispatcher.OnNavigationCanceled(navigationContext);
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
                    dispatcher.OnNavigationCanceled(navigationContext);
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