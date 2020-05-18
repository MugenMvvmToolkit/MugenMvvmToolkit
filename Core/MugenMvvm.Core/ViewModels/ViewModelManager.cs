using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
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
        public ViewModelManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext OnLifecycleChanged<TState>(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewModelLifecycleDispatcherComponent>(metadata).OnLifecycleChanged(viewModel, lifecycleState, state, metadata).DefaultIfNull();
        }

        public object GetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IViewModelServiceResolverComponent>(metadata).TryGetService(viewModel, service, metadata);
            if (result == null)
                ExceptionManager.ThrowCannotResolveService(service);
            return result;
        }

        public IViewModelBase? TryGetViewModel<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewModelProviderComponent>(metadata).TryGetViewModel(request, metadata);
        }

        #endregion
    }
}