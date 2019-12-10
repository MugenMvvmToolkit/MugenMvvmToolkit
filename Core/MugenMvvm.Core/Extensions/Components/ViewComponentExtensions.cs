using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ViewComponentExtensions
    {
        #region Methods

        public static IReadOnlyList<IViewInfo>? TryGetViews(this IViewInfoProviderComponent[] components, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetViews(viewModel, metadata);

            List<IViewInfo>? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                var views = components[i].TryGetViews(viewModel, metadata);
                if (views == null || views.Count == 0)
                    continue;
                if (result == null)
                    result = new List<IViewInfo>();
                result.AddRange(views);
            }

            return result;
        }

        public static IReadOnlyList<IViewInitializer>? TryGetInitializersByView(this IViewInitializerProviderComponent[] components, object view, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetInitializersByView(view, metadata);

            List<IViewInitializer>? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                var initializers = components[i].TryGetInitializersByView(view, metadata);
                if (initializers == null || initializers.Count == 0)
                    continue;
                if (result == null)
                    result = new List<IViewInitializer>();
                result.AddRange(initializers);
            }

            return result;
        }

        public static IReadOnlyList<IViewInitializer>? TryGetInitializersByViewModel(this IViewInitializerProviderComponent[] components, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetInitializersByViewModel(viewModel, metadata);

            List<IViewInitializer>? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                var initializers = components[i].TryGetInitializersByViewModel(viewModel, metadata);
                if (initializers == null || initializers.Count == 0)
                    continue;
                if (result == null)
                    result = new List<IViewInitializer>();
                result.AddRange(initializers);
            }

            return result;
        }

        public static IViewInitializerResult? TryInitialize(this IViewInitializerComponent[] components, IViewInitializer initializer, IViewModelBase? viewModel, object? view, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryInitialize(initializer, viewModel, view, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IReadOnlyMetadataContext? TryCleanup(this IViewInitializerComponent[] components, IViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryCleanup(initializer, viewInfo, viewModel, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static object? TryGetViewForViewModel(this IViewProviderComponent[] components, IViewInitializer initializer, IViewModelBase viewModel, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var view = components[i].TryGetViewForViewModel(initializer, viewModel, metadata);
                if (view != null)
                    return view;
            }

            return null;
        }

        public static IViewModelBase? TryGetViewModelForView(this IViewModelProviderViewManagerComponent[] components, IViewInitializer initializer, object view, IMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var viewModel = components[i].TryGetViewModelForView(initializer, view, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            return null;
        }

        public static void OnViewInitialized(this IViewManagerListener[] listeners, IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewInitialized(viewManager, viewInfo, viewModel, metadata);
        }

        public static void OnViewCleared(this IViewManagerListener[] listeners, IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewCleared(viewManager, viewInfo, viewModel, metadata);
        }

        #endregion
    }
}