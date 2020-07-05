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

        public static void OnLifecycleChanged<TState>(this IViewModelLifecycleDispatcherComponent[] components, IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(viewModelManager, viewModel, lifecycleState, state, metadata);
        }

        public static object? TryGetService<TRequest>(this IViewModelServiceResolverComponent[] components, IViewModelManager viewModelManager, IViewModelBase viewModel, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            Should.NotBeNull(viewModel, nameof(viewModel));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetService(viewModelManager, viewModel, request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IViewModelBase? TryGetViewModel<TRequest>(this IViewModelProviderComponent[] components, IViewModelManager viewModelManager, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            for (var i = 0; i < components.Length; i++)
            {
                var viewModel = components[i].TryGetViewModel(viewModelManager, request, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            return null;
        }

        #endregion
    }
}