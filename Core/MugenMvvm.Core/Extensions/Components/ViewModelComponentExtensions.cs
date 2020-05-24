using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ViewModelComponentExtensions
    {
        #region Methods

        public static void OnLifecycleChanged<TState>(this IViewModelLifecycleDispatcherComponent[] components, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(viewModel, lifecycleState, state, metadata);
        }

        public static object? TryGetService(this IViewModelServiceResolverComponent[] components, IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(service, nameof(service));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetService(viewModel, service, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IViewModelBase? TryGetViewModel<TRequest>(this IViewModelProviderComponent[] components, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var viewModel = components[i].TryGetViewModel(request, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            return null;
        }

        #endregion
    }
}