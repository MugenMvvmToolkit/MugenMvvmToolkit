using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Extensions.Components
{
    public static class PresenterComponentExtensions
    {
        #region Methods
        //todo review
        // public static IViewModelPresenterMediator? TryGetMediator(this IViewModelMediatorProviderComponent[] components, IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext? metadata)
        // {
        //     Should.NotBeNull(components, nameof(components));
        //     for (var i = 0; i < components.Length; i++)
        //     {
        //         var mediator = components[i].TryGetMediator(viewModel, viewInitializer, metadata)!;
        //         if (mediator != null)
        //             return mediator;
        //     }
        //
        //     return null;
        // }
        //
        // public static IReadOnlyList<IPresenterResult>? TryClose(this IViewModelMediatorCloseManagerComponent[] components, IViewModelBase viewModel, IReadOnlyList<IViewModelPresenterMediator> mediators,
        //     IReadOnlyMetadataContext? metadata)
        // {
        //     Should.NotBeNull(components, nameof(components));
        //     for (var i = 0; i < components.Length; i++)
        //     {
        //         var result = components[i].TryClose(viewModel, mediators, metadata);
        //         if (result != null)
        //             return result;
        //     }
        //
        //     return null;
        // }

        public static void OnCallbackAdded(this IViewModelCallbackManagerListener[] listeners, INavigationCallback callback, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCallbackAdded(callback, viewModel, metadata);
        }

        public static void OnCallbackExecuted(this IViewModelCallbackManagerListener[] listeners, INavigationCallback callback, IViewModelBase viewModel, INavigationContext? context)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCallbackExecuted(callback, viewModel, context);
        }

        public static bool CanShow(this IConditionPresenterComponent[] components, IPresenterComponent component, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanShow(component, metadata))
                    return false;
            }

            return true;
        }

        public static bool CanClose(this IConditionPresenterComponent[] components, ICloseablePresenterComponent component, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanClose(component, results, metadata))
                    return false;
            }

            return true;
        }

        public static bool CanRestore(this IConditionPresenterComponent[] components, IRestorablePresenterComponent component, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanRestore(component, results, metadata))
                    return false;
            }

            return true;
        }

        public static IPresenterResult? TryShow(this IPresenterComponent[] components, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryShow(metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IReadOnlyList<IPresenterResult>? TryClose(this ICloseablePresenterComponent[] components, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            List<IPresenterResult>? results = null;
            for (var i = 0; i < components.Length; i++)
            {
                var operations = components[i].TryClose(metadata);
                if (operations == null || operations.Count == 0)
                    continue;
                if (results == null)
                    results = new List<IPresenterResult>();
                results.AddRange(operations);
            }

            return results;
        }

        public static IReadOnlyList<IPresenterResult>? TryRestore(this IRestorablePresenterComponent[] components, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            List<IPresenterResult>? results = null;
            for (var i = 0; i < components.Length; i++)
            {
                var operations = components[i].TryRestore(metadata);
                if (operations == null || operations.Count == 0)
                    continue;
                if (results == null)
                    results = new List<IPresenterResult>();
                results.AddRange(operations);
            }

            return results;
        }

        #endregion
    }
}