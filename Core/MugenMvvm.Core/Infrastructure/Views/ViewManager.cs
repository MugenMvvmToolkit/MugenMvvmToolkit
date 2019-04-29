using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
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
        public ViewManager(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            ComponentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        public bool IsListenersInitialized => _listeners != null;

        public IComponentCollection<IViewManagerListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    ComponentCollectionProvider.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        public IComponentCollection<IChildViewManager> Managers
        {
            get
            {
                if (_managers == null)
                    ComponentCollectionProvider.LazyInitialize(ref _managers, this);
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

        void IParentViewManager.OnViewCreated(object view, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            OnViewCreated(view, viewModel, metadata);
        }

        void IParentViewManager.OnViewInitialized(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(metadata, nameof(metadata));
            OnViewInitialized(viewInfo, viewModel, metadata);
        }

        void IParentViewManager.OnViewCleared(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(metadata, nameof(metadata));
            OnViewCleared(viewInfo, viewModel, metadata);
        }

        #endregion

        #region Methods

        protected virtual IReadOnlyList<IViewInfo> GetViewsInternal(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            if (managers.Length == 0)
                return Default.EmptyArray<IViewInfo>();
            if (managers.Length == 1)
                return managers[0].GetViews(this, viewModel, metadata);

            var result = new List<IViewInfo>();
            for (var i = 0; i < managers.Length; i++)
                result.AddRange(managers[i].GetViews(this, viewModel, metadata));
            return result;
        }

        protected virtual IReadOnlyList<IViewModelViewInitializer> GetInitializersByViewInternal(object view, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            if (managers.Length == 0)
                return Default.EmptyArray<IViewModelViewInitializer>();
            if (managers.Length == 1)
                return managers[0].GetInitializersByView(this, view, metadata);

            var result = new List<IViewModelViewInitializer>();
            for (var i = 0; i < managers.Length; i++)
                result.AddRange(managers[i].GetInitializersByView(this, view, metadata));
            return result;
        }

        protected virtual IReadOnlyList<IViewInitializer> GetInitializersByViewModelInternal(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItems();
            if (managers.Length == 0)
                return Default.EmptyArray<IViewInitializer>();
            if (managers.Length == 1)
                return managers[0]?.GetInitializersByViewModel(this, viewModel, metadata) ?? Default.EmptyArray<IViewInitializer>();

            var result = new List<IViewInitializer>();
            for (var i = 0; i < managers.Length; i++)
                result.AddRange(managers[i].GetInitializersByViewModel(this, viewModel, metadata));
            return result;
        }

        protected virtual void OnViewModelCreated(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewModelCreated(this, viewModel, view, metadata);
        }

        protected virtual void OnViewCreated(object view, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewCreated(this, view, viewModel, metadata);
        }

        protected virtual void OnViewInitialized(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewInitialized(this, viewInfo, viewModel, metadata);
        }

        protected virtual void OnViewCleared(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnViewCleared(this, viewInfo, viewModel, metadata);
        }

        #endregion
    }
}