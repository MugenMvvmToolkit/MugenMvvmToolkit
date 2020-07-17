using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ViewModelComponentExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnLifecycleChanged(this IViewModelLifecycleDispatcherComponent[] components, IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            object? state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(viewModelManager, viewModel, lifecycleState, state, metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? TryGetService(this IViewModelServiceResolverComponent[] components, IViewModelManager viewModelManager, IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(request, nameof(request));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetService(viewModelManager, viewModel, request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IViewModelBase? TryGetViewModel(this IViewModelProviderComponent[] components, IViewModelManager viewModelManager, object request, IReadOnlyMetadataContext? metadata)
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