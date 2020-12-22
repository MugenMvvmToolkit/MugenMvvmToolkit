using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.ViewModels
{
    public sealed class ViewModelManager : ComponentOwnerBase<IViewModelManager>, IViewModelManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool IsInState(IViewModelBase viewModel, ViewModelLifecycleState state, IReadOnlyMetadataContext? metadata = null)
            => GetComponents<ILifecycleTrackerComponent<ViewModelLifecycleState>>(metadata).IsInState(this, viewModel, state, metadata);

        public void OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state = null, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IViewModelLifecycleListener>(metadata).OnLifecycleChanged(this, viewModel, lifecycleState, state, metadata);

        public object? TryGetService(IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IViewModelServiceResolverComponent>(metadata).TryGetService(this, viewModel, request, metadata);

        public IViewModelBase? TryGetViewModel(object request, IReadOnlyMetadataContext? metadata = null) => GetComponents<IViewModelProviderComponent>(metadata).TryGetViewModel(this, request, metadata);

        #endregion
    }
}