﻿using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Views
{
    public class ViewManager : IParentViewManager
    {
        #region Fields

        private IComponentCollection<IViewManagerListener>? _listeners;
        private IComponentCollection<IChildViewManager>? _managers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManager(IComponentCollection<IChildViewManager>? managers = null, IComponentCollection<IViewManagerListener>? listeners = null)
        {
            _managers = managers;
            _listeners = listeners;
        }

        #endregion

        #region Properties

        public IComponentCollection<IViewManagerListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    _listeners = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IViewManagerListener>(this, Default.MetadataContext);
                return _listeners;
            }
        }

        public IComponentCollection<IChildViewManager> Managers
        {
            get
            {
                if (_managers == null)
                    _managers = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IChildViewManager>(this, Default.MetadataContext);
                return _managers;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewInfo> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetViewsInternal(viewModel, metadata);
        }

        public IReadOnlyList<IViewModelViewInitializer> GetInitializersByView(object view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetInitializersByViewInternal(view, metadata);
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetInitializersByViewModelInternal(viewModel, metadata);
        }

        void IParentViewManager.OnViewModelCreated(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            OnViewModelCreated(viewModel, view, metadata);
        }

        void IParentViewManager.OnViewCreated(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            OnViewCreated(viewModel, view, metadata);
        }

        void IParentViewManager.OnViewInitialized(IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(metadata, nameof(metadata));
            OnViewInitialized(viewModel, viewInfo, metadata);
        }

        void IParentViewManager.OnViewCleared(IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(metadata, nameof(metadata));
            OnViewCleared(viewModel, viewInfo, metadata);
        }

        #endregion

        #region Methods

        protected virtual IReadOnlyList<IViewInfo> GetViewsInternal(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            if (managers.Count == 0)
                return Default.EmptyArray<IViewInfo>();
            if (managers.Count == 1)
                return managers[0].GetViews(this, viewModel, metadata);

            var result = new List<IViewInfo>();
            for (var i = 0; i < managers.Count; i++)
                result.AddRange(managers[i].GetViews(this, viewModel, metadata));
            return result;
        }

        protected virtual IReadOnlyList<IViewModelViewInitializer> GetInitializersByViewInternal(object view, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            if (managers.Count == 0)
                return Default.EmptyArray<IViewModelViewInitializer>();
            if (managers.Count == 1)
                return managers[0].GetInitializersByView(this, view, metadata);

            var result = new List<IViewModelViewInitializer>();
            for (var i = 0; i < managers.Count; i++)
                result.AddRange(managers[i].GetInitializersByView(this, view, metadata));
            return result;
        }

        protected virtual IReadOnlyList<IViewInitializer> GetInitializersByViewModelInternal(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            if (managers.Count == 0)
                return Default.EmptyArray<IViewInitializer>();
            if (managers.Count == 1)
                return managers[0]?.GetInitializersByViewModel(this, viewModel, metadata) ?? Default.EmptyArray<IViewInitializer>();

            var result = new List<IViewInitializer>();
            for (var i = 0; i < managers.Count; i++)
                result.AddRange(managers[i].GetInitializersByViewModel(this, viewModel, metadata));
            return result;
        }

        protected virtual void OnViewModelCreated(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnViewModelCreated(this, viewModel, view, metadata);
        }

        protected virtual void OnViewCreated(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnViewCreated(this, viewModel, view, metadata);
        }

        protected virtual void OnViewInitialized(IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnViewInitialized(this, viewModel, viewInfo, metadata);
        }

        protected virtual void OnViewCleared(IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnViewCleared(this, viewModel, viewInfo, metadata);
        }

        #endregion
    }
}