using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public sealed class CacheViewModelDispatcherComponent : IViewModelProviderViewModelDispatcherComponent, IObservableMetadataContextListener
    {
        #region Fields

        private readonly bool _isWeakCache;
        private static readonly Guid DefaultId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);

        private readonly Dictionary<Guid, object> _viewModelsCache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public CacheViewModelDispatcherComponent(bool isWeakCache = true)
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
            if (!ViewModelMetadata.Id.Equals(key))
                return;

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

        void IObservableMetadataContextListener.OnRemoved(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
        {
            if (ViewModelMetadata.Id.Equals(key))
                RemoveFromCache(ViewModelMetadata.Id.GetValue(metadataContext, oldValue));
        }

        public IReadOnlyMetadataContext? OnLifecycleChanged(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            IReadOnlyMetadataContext metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Created)
            {
                AddToCache(viewModel.Metadata.Get(ViewModelMetadata.Id), viewModel);
                viewModel.Metadata.AddListener(this);
            }
            else if (lifecycleState.IsDispose)
            {
                RemoveFromCache(viewModel.Metadata.Get(ViewModelMetadata.Id));
                viewModel.Metadata.RemoveListener(this);
            }

            return null;
        }

        public IViewModelBase? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, IReadOnlyMetadataContext metadata)
        {
            var id = metadata.Get(ViewModelMetadata.Id, DefaultId);
            if (id == DefaultId)
                return null;

            object value;
            lock (_viewModelsCache)
            {
                if (!_viewModelsCache.TryGetValue(id, out value))
                    return null;
            }

            if (!_isWeakCache)
                return (IViewModelBase)value;

            var vm = (IViewModelBase)((IWeakReference)value).Target;
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