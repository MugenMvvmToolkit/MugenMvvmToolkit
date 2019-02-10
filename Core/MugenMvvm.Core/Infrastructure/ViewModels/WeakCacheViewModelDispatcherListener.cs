using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public sealed class WeakCacheViewModelDispatcherListener : IViewModelDispatcherListener, IObservableMetadataContextListener
    {
        #region Fields

        private readonly Dictionary<Guid, WeakReference> _viewModelsCache;

        #endregion

        #region Constructors

        public WeakCacheViewModelDispatcherListener()
        {
            _viewModelsCache = new Dictionary<Guid, WeakReference>();
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

        public IViewModel? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, Guid id, IReadOnlyMetadataContext metadata)
        {
            WeakReference value;
            lock (_viewModelsCache)
            {
                if (!_viewModelsCache.TryGetValue(id, out value))
                    return null;
            }

            var vm = (IViewModel)value.Target;
            if (vm == null)
                RemoveFromCache(id);
            return vm;
        }

        public void OnSubscribe(IViewModelDispatcher viewModelDispatcher, IViewModel viewModel, object observer, ThreadExecutionMode executionMode,
            IReadOnlyMetadataContext metadata)
        {
        }

        public void OnUnsubscribe(IViewModelDispatcher viewModelDispatcher, IViewModel viewModel, object observer, IReadOnlyMetadataContext metadata)
        {
        }

        public void OnLifecycleChanged(IViewModelDispatcher viewModelDispatcher, IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Created)
            {
                AddToCache(viewModel.Metadata.Get(ViewModelMetadata.Id), viewModel);
                if (viewModel.Metadata is MetadataContext ctx)
                    ctx.InternalListener = this;
                else
                    viewModel.Metadata.AddListener(this);
            }
            else if (lifecycleState.IsDispose)
                RemoveFromCache(viewModel.Metadata.Get(ViewModelMetadata.Id));
        }

        #endregion

        #region Methods

        private void AddToCache(Guid id, IViewModel viewModel)
        {
            lock (_viewModelsCache)
            {
                _viewModelsCache[id] = MugenExtensions.GetWeakReference(viewModel);
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