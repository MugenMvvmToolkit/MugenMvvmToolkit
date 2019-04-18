using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public sealed class CacheViewModelDispatcherManager : IViewModelProviderViewModelDispatcherManager, IObservableMetadataContextListener
    {
        #region Fields

        private readonly bool _isWeakCache;

        private readonly Dictionary<Guid, object> _viewModelsCache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public CacheViewModelDispatcherManager(bool isWeakCache = true)
        {
            _isWeakCache = isWeakCache;
            _viewModelsCache = new Dictionary<Guid, object>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public int GetPriority(object source)
        {
            return 0;
        }

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

        public void OnLifecycleChanged(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            IReadOnlyMetadataContext metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Created)
            {
                AddToCache(viewModel.Metadata.Get(ViewModelMetadata.Id), viewModel);
                viewModel.Metadata.Listeners.Add(this);
            }
            else if (lifecycleState.IsDispose)
            {
                RemoveFromCache(viewModel.Metadata.Get(ViewModelMetadata.Id));
                viewModel.Metadata.Listeners.Remove(this);
            }
        }

        public IViewModelBase? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, Type viewModelType, IReadOnlyMetadataContext metadata)
        {
            return null;
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
                return (IViewModelBase) value;

            var vm = (IViewModelBase) ((WeakReference) value).Target;
            if (vm == null)
                RemoveFromCache(id);
            return vm;
        }

        #endregion

        #region Methods

        private void AddToCache(Guid id, IViewModelBase viewModel)
        {
            lock (_viewModelsCache)
            {
                _viewModelsCache[id] = _isWeakCache ? (object) MugenExtensions.GetWeakReference(viewModel) : viewModel;
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