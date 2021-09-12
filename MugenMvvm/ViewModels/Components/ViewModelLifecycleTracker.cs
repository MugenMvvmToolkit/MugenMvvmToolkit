using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class ViewModelLifecycleTracker : IViewModelLifecycleListener, ILifecycleTrackerComponent<IViewModelManager, ViewModelLifecycleState>, IHasPriority
    {
        public int Priority { get; init; } = ViewModelComponentPriority.LifecycleTracker;

        public bool IsInState(IViewModelManager owner, object target, ViewModelLifecycleState state, IReadOnlyMetadataContext? metadata)
        {
            if (state == ViewModelLifecycleState.Created)
                return true;
            if (state == ViewModelLifecycleState.Initialized)
            {
                if (target is ViewModelBase vm)
                    return vm.IsInitialized;
                return ((IViewModelBase)target).Metadata.TryGet(InternalMetadata.IsInitialized, out var v) && v;
            }

            if (state == ViewModelLifecycleState.Disposed)
            {
                if (target is IHasDisposeState vm)
                    return vm.IsDisposed;
                return ((IViewModelBase)target).Metadata.TryGet(InternalMetadata.IsDisposed, out var v) && v;
            }

            return false;
        }

        public void OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Disposed && viewModel is not ViewModelBase)
                viewModel.Metadata.Set(InternalMetadata.IsDisposed, true, out _);
            else if (lifecycleState == ViewModelLifecycleState.Initialized)
            {
                if (viewModel is ViewModelBase vm)
                    vm.IsInitialized = true;
                else
                    viewModel.Metadata.Set(InternalMetadata.IsInitialized, true, out _);
            }
        }
    }
}