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
    public sealed class ViewModelLifecycleTracker : IViewModelLifecycleListener, ILifecycleTrackerComponent<ViewModelLifecycleState>, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewModelComponentPriority.LifecycleTracker;

        #endregion

        #region Implementation of interfaces

        public bool IsInState(object owner, object target, ViewModelLifecycleState state, IReadOnlyMetadataContext? metadata)
        {
            if (state == ViewModelLifecycleState.Created)
                return true;
            if (state == ViewModelLifecycleState.Disposed)
            {
                if (target is ViewModelBase vm)
                    return vm.IsDisposed;
                return ((IViewModelBase) target).Metadata.TryGet(InternalMetadata.IsDisposed, out var v) && v;
            }

            return false;
        }

        public void OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Disposed && !(viewModel is ViewModelBase))
                viewModel.Metadata.Set(InternalMetadata.IsDisposed, true, out _);
        }

        #endregion
    }
}