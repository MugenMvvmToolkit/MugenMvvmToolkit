using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public sealed class CacheViewModelDispatcherListener : IViewModelDispatcherListener, IObservableMetadataContextListener
    {
        #region Fields

        private readonly Dictionary<Guid, object> _viewModelsCache;
        private readonly bool _isWeakCache;

        #endregion

        #region Constructors

        public CacheViewModelDispatcherListener(bool isIsWeakCache = true)
        {
            _isWeakCache = isIsWeakCache;
            _viewModelsCache = new Dictionary<Guid, object>();
        }

        #endregion

        #region Implementation of interfaces

        void IObservableMetadataContextListener.OnAdded(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? newValue)
        {
        }

        void IObservableMetadataContextListener.OnChanged(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue)
        {
            if (ViewModelMetadata.Id.Equals(key))
            {
                lock (_viewModelsCache)
                {
                    var oldId = ViewModelMetadata.Id.GetValue(metadataContext, oldValue);
                    if (_viewModelsCache.TryGetValue(oldId, out var value))
                    {
                        _viewModelsCache.Remove(oldId);
                        _viewModelsCache[ViewModelMetadata.Id.GetValue(metadataContext, newValue)] = value;
                    }
                }
            }
        }

        void IObservableMetadataContextListener.OnRemoved(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
        {
            if (ViewModelMetadata.Id.Equals(key))
                RemoveFromCache(ViewModelMetadata.Id.GetValue(metadataContext, oldValue));
        }

        public IViewModelBase? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, Guid id, IReadOnlyMetadataContext metadata)
        {
            object value;
            lock (_viewModelsCache)
            {
                if (!_viewModelsCache.TryGetValue(id, out value))
                    return null;
            }

            if (!_isWeakCache)
                return (IViewModelBase)value;

            var vm = (IViewModelBase)((WeakReference)value).Target;
            if (vm == null)
                RemoveFromCache(id);
            return vm;
        }

        public bool OnSubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode,
            IReadOnlyMetadataContext metadata)
        {
            return false;
        }

        public bool OnUnsubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata)
        {
            return false;
        }

        public void OnLifecycleChanged(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            IReadOnlyMetadataContext metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Created)
            {
                AddToCache(viewModel.Metadata.Get(ViewModelMetadata.Id), viewModel);
                viewModel.Metadata.AddListener(this);
            }
            else if (lifecycleState.IsDispose)
                RemoveFromCache(viewModel.Metadata.Get(ViewModelMetadata.Id));
        }

        #endregion

        #region Methods

        private void AddToCache(Guid id, IViewModelBase viewModel)
        {
            lock (_viewModelsCache)
            {
                _viewModelsCache[id] = _isWeakCache ? (object)MugenExtensions.GetWeakReference(viewModel) : viewModel;
            }
        }

        private void RemoveFromCache(Guid id)
        {
            lock (_viewModelsCache)
            {
                _viewModelsCache.Remove(id);
            }
        }

        #endregion
    }
}