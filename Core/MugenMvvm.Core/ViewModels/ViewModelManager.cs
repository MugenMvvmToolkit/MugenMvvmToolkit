using System;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.ViewModels
{
    public class ViewModelManager : ComponentOwnerBase<IViewModelManager>, IViewModelManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            return OnLifecycleChangedInternal(viewModel, lifecycleState, metadata).DefaultIfNull();
        }

        public object GetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(service, nameof(service));
            var result = GetServiceInternal(viewModel, service, metadata);
            if (result == null)
                ExceptionManager.ThrowIocCannotFindBinding(service);

            return result!;
        }

        public IViewModelBase? TryGetViewModel(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return TryGetViewModelInternal(metadata);
        }

        #endregion

        #region Methods

        protected virtual IReadOnlyMetadataContext? OnLifecycleChangedInternal(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext? metadata)
        {
            //            if (lifecycleState != ViewModelLifecycleState.Finalized)
            //                viewModel.Metadata.Set(ViewModelMetadata.LifecycleState, lifecycleState);//todo move to component
            IReadOnlyMetadataContext? result = null;
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var m = (components[i] as IViewModelLifecycleDispatcherComponent)?.OnLifecycleChanged(viewModel, lifecycleState, metadata);
                m.Aggregate(ref result);
            }

            return result;
        }

        protected virtual object? GetServiceInternal(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                var result = (components[i] as IViewModelServiceResolverComponent)?.TryGetService(viewModel, service, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        protected virtual IViewModelBase? TryGetViewModelInternal(IReadOnlyMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                var viewModel = (components[i] as IViewModelProviderComponent)?.TryGetViewModel(metadata);
                if (viewModel != null)
                    return viewModel;
            }

            return null;
        }

        #endregion
    }
}