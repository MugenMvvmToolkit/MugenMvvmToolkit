﻿using System;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
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
                    _managers = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IViewModelDispatcherManager>(this, Default.MetadataContext);
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
                throw ExceptionManager.IoCCannotFindBinding(service);
            return result;
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

        public IViewModelBase GetViewModel(Type vmType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(vmType, nameof(vmType));
            Should.NotBeNull(metadata, nameof(metadata));
            var vm = GetViewModelInternal(vmType, metadata);
            if (vm == null)
                throw ExceptionManager.CannotGetViewModel(vmType);
            return vm;
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
            for (var i = 0; i < managers.Count; i++)
                managers[i].OnLifecycleChanged(this, viewModel, lifecycleState, metadata);
        }

        protected virtual object? GetServiceInternal(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Count; i++)
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
            for (var i = 0; i < managers.Count; i++)
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
            for (var i = 0; i < managers.Count; i++)
            {
                var result = (managers[i] as ISubscriberViewModelDispatcherManager)?.TryUnsubscribe(this, viewModel, observer, metadata);
                if (result.GetValueOrDefault())
                    unsubscribed = true;
            }

            return unsubscribed;
        }

        protected virtual IViewModelBase? GetViewModelInternal(Type vmType, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Count; i++)
            {
                var viewModel = (managers[i] as IViewModelProviderViewModelDispatcherManager)?.TryGetViewModel(this, vmType, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            return null;
        }

        protected virtual IViewModelBase? TryGetViewModelInternal(Guid id, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Count; i++)
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