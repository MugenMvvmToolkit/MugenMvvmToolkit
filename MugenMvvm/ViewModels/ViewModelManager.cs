using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
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

        public void OnLifecycleChanged<TState>(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            GetComponents<IViewModelLifecycleDispatcherComponent>(metadata).OnLifecycleChanged(this, viewModel, lifecycleState, state, metadata);
        }

        public object? TryGetService<TRequest>(IViewModelBase viewModel, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewModelServiceResolverComponent>(metadata).TryGetService(this, viewModel, request, metadata);
        }

        public IViewModelBase? TryGetViewModel<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewModelProviderComponent>(metadata).TryGetViewModel(this, request, metadata);
        }

        #endregion
    }
}