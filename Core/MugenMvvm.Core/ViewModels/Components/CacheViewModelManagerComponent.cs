using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class CacheViewModelManagerComponent : IViewModelProviderComponent, IMetadataContextListener, IViewModelLifecycleDispatcherComponent, IHasComponentPriority, IHasPriority
    {
        #region Fields

        private readonly bool _isWeakCache;

        private readonly Dictionary<Guid, object> _viewModelsCache;
        private static readonly Guid DefaultId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);//todo review vmid

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public CacheViewModelManagerComponent(bool isWeakCache = true)
        {
            _isWeakCache = isWeakCache;
            _viewModelsCache = new Dictionary<Guid, object>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IMetadataContextListener.OnAdded(IMetadataContext metadataContext, IMetadataContextKey key, object? newValue)
        {
        }

        void IMetadataContextListener.OnChanged(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue)
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

        void IMetadataContextListener.OnRemoved(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
        {
            if (ViewModelMetadata.Id.Equals(key))
                RemoveFromCache(ViewModelMetadata.Id.GetValue(metadataContext, oldValue));
        }

        public IReadOnlyMetadataContext? OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Created)
            {
                AddToCache(viewModel.Metadata.Get(ViewModelMetadata.Id), viewModel);
                viewModel.Metadata.AddComponent(this);
            }
            else if (lifecycleState.IsDispose)
            {
                RemoveFromCache(viewModel.Metadata.Get(ViewModelMetadata.Id));
                viewModel.Metadata.RemoveComponent(this);
            }

            return null;
        }

        public int GetPriority(object owner)
        {
            if (owner is IViewModelManager)
                return Priority;
            return 0;
        }

        public IViewModelBase? TryGetViewModel(IReadOnlyMetadataContext metadata)
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

            var vm = (IViewModelBase?)((IWeakReference)value).Target;
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
                _viewModelsCache[id] = _isWeakCache ? (object)viewModel.ToWeakReference() : viewModel;
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