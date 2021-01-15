using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ViewModelComponentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnLifecycleChanged(this ItemOrArray<IViewModelLifecycleListener> components, IViewModelManager viewModelManager, IViewModelBase viewModel,
            ViewModelLifecycleState lifecycleState,
            object? state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            foreach (var c in components)
                c.OnLifecycleChanged(viewModelManager, viewModel, lifecycleState, state, metadata);
        }

        public static object? TryGetService(this ItemOrArray<IViewModelServiceProviderComponent> components, IViewModelManager viewModelManager, IViewModelBase viewModel,
            object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(request, nameof(request));
            foreach (var c in components)
            {
                var result = c.TryGetService(viewModelManager, viewModel, request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IViewModelBase? TryGetViewModel(this ItemOrArray<IViewModelProviderComponent> components, IViewModelManager viewModelManager, object request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            foreach (var c in components)
            {
                var viewModel = c.TryGetViewModel(viewModelManager, request, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            return null;
        }
    }
}