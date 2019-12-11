using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Views;

namespace MugenMvvm.Extensions.Components
{
    public static class ViewComponentExtensions
    {
        #region Methods

        public static IReadOnlyList<IView>? TryGetViews(this IViewProviderComponent[] components, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetViews(viewModel, metadata);

            List<IView>? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                var views = components[i].TryGetViews(viewModel, metadata);
                if (views == null || views.Count == 0)
                    continue;
                if (result == null)
                    result = new List<IView>();
                result.AddRange(views);
            }

            return result;
        }

        public static IReadOnlyList<IViewModelViewMapping>? TryGetMappingByView(this IViewModelViewMappingProviderComponent[] components, object view, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetMappingByView(view, metadata);

            List<IViewModelViewMapping>? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                var initializers = components[i].TryGetMappingByView(view, metadata);
                if (initializers == null || initializers.Count == 0)
                    continue;
                if (result == null)
                    result = new List<IViewModelViewMapping>();
                result.AddRange(initializers);
            }

            return result;
        }

        public static IReadOnlyList<IViewModelViewMapping>? TryGetMappingByViewModel(this IViewModelViewMappingProviderComponent[] components, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetMappingByViewModel(viewModel, metadata);

            List<IViewModelViewMapping>? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                var initializers = components[i].TryGetMappingByViewModel(viewModel, metadata);
                if (initializers == null || initializers.Count == 0)
                    continue;
                if (result == null)
                    result = new List<IViewModelViewMapping>();
                result.AddRange(initializers);
            }

            return result;
        }

        public static Task<ViewInitializationResult>? TryInitializeAsync(this IViewInitializerComponent[] components, IViewModelViewMapping mapping, object? view, IViewModelBase? viewModel,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryInitializeAsync(mapping, view, viewModel, cancellationToken, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static Task? TryCleanupAsync(this IViewInitializerComponent[] components, IView view, IViewModelBase? viewModel, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryCleanupAsync(view, viewModel, cancellationToken, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnViewInitialized(this IViewManagerListener[] listeners, IViewManager viewManager, IView view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewInitialized(viewManager, view, viewModel, metadata);
        }

        public static void OnViewCleared(this IViewManagerListener[] listeners, IViewManager viewManager, IView view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewCleared(viewManager, view, viewModel, metadata);
        }

        #endregion
    }
}