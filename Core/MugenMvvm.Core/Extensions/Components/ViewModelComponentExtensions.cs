
using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ViewModelComponentExtensions
    {
        #region Methods

        public static IReadOnlyMetadataContext? OnLifecycleChanged(this IViewModelLifecycleDispatcherComponent[] components, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            IReadOnlyMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(viewModel, lifecycleState, metadata).Aggregate(ref result);
            return result;
        }

        public static object? TryGetService(this IViewModelServiceResolverComponent[] components, IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetService(viewModel, service, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IViewModelBase? TryGetViewModel(this IViewModelProviderComponent[] components, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var viewModel = components[i].TryGetViewModel(metadata);
                if (viewModel != null)
                    return viewModel;
            }

            return null;
        }

        #endregion
    }
}