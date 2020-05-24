using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;
using MugenMvvm.Views;

namespace MugenMvvm.Extensions.Components
{
    public static class ViewComponentExtensions
    {
        #region Methods

        public static IReadOnlyList<IView>? TryGetViews(this IViewProviderComponent[] components, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewModel, nameof(viewModel));
            if (components.Length == 1)
                return components[0].TryGetViews(viewModel, metadata);

            LazyList<IView> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetViews(viewModel, metadata));
            return result.List;
        }

        public static IReadOnlyList<IViewModelViewMapping>? TryGetMappingByView(this IViewModelViewMappingProviderComponent[] components, object view, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(view, nameof(view));
            if (components.Length == 1)
                return components[0].TryGetMappingByView(view, metadata);

            LazyList<IViewModelViewMapping> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMappingByView(view, metadata));
            return result.List;
        }

        public static IReadOnlyList<IViewModelViewMapping>? TryGetMappingByViewModel(this IViewModelViewMappingProviderComponent[] components, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewModel, nameof(viewModel));
            if (components.Length == 1)
                return components[0].TryGetMappingByViewModel(viewModel, metadata);

            LazyList<IViewModelViewMapping> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMappingByViewModel(viewModel, metadata));
            return result.List;
        }

        public static Task<ViewInitializationResult>? TryInitializeAsync(this IViewInitializerComponent[] components, IViewModelViewMapping mapping, object? view,
            IViewModelBase? viewModel, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(mapping, nameof(mapping));
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
            Should.NotBeNull(view, nameof(view));
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
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewModel, nameof(viewModel));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewInitialized(viewManager, view, viewModel, metadata);
        }

        public static void OnViewCleared(this IViewManagerListener[] listeners, IViewManager viewManager, IView view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewModel, nameof(viewModel));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewCleared(viewManager, view, viewModel, metadata);
        }

        #endregion
    }
}