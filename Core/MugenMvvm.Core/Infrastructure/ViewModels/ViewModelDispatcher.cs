using System;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public class ViewModelDispatcher : IViewModelDispatcher
    {
        #region Fields

        private IComponentCollection<IViewModelDispatcherManager>? _managers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelDispatcher(IComponentCollection<IViewModelDispatcherManager>? managers = null)
        {
            _managers = managers;
        }

        #endregion

        #region Properties

        public IComponentCollection<IViewModelDispatcherManager> Managers
        {
            get
            {
                if (_managers == null)
                    MugenExtensions.LazyInitialize(ref _managers, this);
                return _managers;
            }
        }

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            Should.NotBeNull(metadata, nameof(metadata));
            OnLifecycleChangedInternal(viewModel, lifecycleState, metadata);
        }

        public object GetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(metadata, nameof(metadata));
            var result = GetServiceInternal(viewModel, service, metadata);
            if (result == null)
                ExceptionManager.ThrowIoCCannotFindBinding(service);

            return result!;
        }

        public bool Subscribe(IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            return SubscribeInternal(viewModel, observer, executionMode, metadata);
        }

        public bool Unsubscribe(IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            return UnsubscribeInternal(viewModel, observer, metadata);
        }

        public IViewModelBase GetViewModel(Type viewModelType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Should.NotBeNull(metadata, nameof(metadata));
            var vm = GetViewModelInternal(viewModelType, metadata);
            if (vm == null)
                ExceptionManager.ThrowCannotGetViewModel(viewModelType);

            return vm!;
        }

        public IViewModelBase? TryGetViewModel(Guid id, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return TryGetViewModelInternal(id, metadata);
        }

        #endregion

        #region Methods

        protected virtual void OnLifecycleChangedInternal(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata)
        {
            if (lifecycleState != ViewModelLifecycleState.Finalized)
                viewModel.Metadata.Set(ViewModelMetadata.LifecycleState, lifecycleState);

            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Length; i++)
                managers[i].OnLifecycleChanged(this, viewModel, lifecycleState, metadata);
        }

        protected virtual object? GetServiceInternal(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Length; i++)
            {
                var result = (managers[i] as IServiceResolverViewModelDispatcherManager)?.TryGetService(this, viewModel, service, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        protected virtual bool SubscribeInternal(IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata)
        {
            var subscribed = false;
            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Length; i++)
            {
                var result = (managers[i] as ISubscriberViewModelDispatcherManager)?.TrySubscribe(this, viewModel, observer, executionMode, metadata);
                if (result.GetValueOrDefault())
                    subscribed = true;
            }

            return subscribed;
        }

        protected virtual bool UnsubscribeInternal(IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata)
        {
            var unsubscribed = false;
            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Length; i++)
            {
                var result = (managers[i] as ISubscriberViewModelDispatcherManager)?.TryUnsubscribe(this, viewModel, observer, metadata);
                if (result.GetValueOrDefault())
                    unsubscribed = true;
            }

            return unsubscribed;
        }

        protected virtual IViewModelBase? GetViewModelInternal(Type viewModelType, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Length; i++)
            {
                var viewModel = (managers[i] as IViewModelProviderViewModelDispatcherManager)?.TryGetViewModel(this, viewModelType, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            return null;
        }

        protected virtual IViewModelBase? TryGetViewModelInternal(Guid id, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Length; i++)
            {
                var viewModel = (managers[i] as IViewModelProviderViewModelDispatcherManager)?.TryGetViewModel(this, id, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            return null;
        }

        #endregion
    }
}