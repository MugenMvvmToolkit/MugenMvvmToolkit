using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Views
{
    public class ViewManager : HasListenersBase<IViewManagerListener>, IParentViewManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManager()
        {
            Managers = new OrderedLightArrayList<IChildViewManager>(HasPriorityComparer.Instance);
        }

        #endregion

        #region Properties

        public LightArrayList<IChildViewManager> Managers { get; }

        #endregion

        #region Implementation of interfaces

        public void AddManager(IChildViewManager manager)
        {
            Should.NotBeNull(manager, nameof(manager));
            AddManagerInternal(manager);
        }

        public void RemoveManager(IChildViewManager manager)
        {
            Should.NotBeNull(manager, nameof(manager));
            RemoveManagerInternal(manager);
        }

        public IReadOnlyList<IChildViewManager> GetManagers()
        {
            return GetManagersInternal();
        }

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

        protected virtual void AddManagerInternal(IChildViewManager manager)
        {
            Managers.AddWithLock(manager);

            var listeners = GetListenersInternal();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnChildViewManagerAdded(this, manager);
        }

        protected virtual void RemoveManagerInternal(IChildViewManager manager)
        {
            Managers.RemoveWithLock(manager);

            var listeners = GetListenersInternal();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnChildViewManagerRemoved(this, manager);
        }

        protected virtual IReadOnlyList<IChildViewManager> GetManagersInternal()
        {
            return Managers.ToArrayWithLock();
        }

        protected virtual IReadOnlyList<IViewInfo> GetViewsInternal(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItemsWithLock(out var size);
            if (size == 0)
                return Default.EmptyArray<IViewInfo>();
            if (size == 1)
                return managers[0].GetViews(this, viewModel, metadata);

            managers = Managers.ToArrayWithLock();
            var result = new List<IViewInfo>();
            for (var i = 0; i < managers.Length; i++)
                result.AddRange(managers[i].GetViews(this, viewModel, metadata));
            return result;
        }

        protected virtual IReadOnlyList<IViewModelViewInitializer> GetInitializersByViewInternal(object view, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItemsWithLock(out var size);
            if (size == 0)
                return Default.EmptyArray<IViewModelViewInitializer>();
            if (size == 1)
                return managers[0].GetInitializersByView(this, view, metadata);

            managers = Managers.ToArrayWithLock();
            var result = new List<IViewModelViewInitializer>();
            for (var i = 0; i < managers.Length; i++)
                result.AddRange(managers[i].GetInitializersByView(this, view, metadata));
            return result;
        }

        protected virtual IReadOnlyList<IViewInitializer> GetInitializersByViewModelInternal(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var managers = Managers.GetItemsWithLock(out var size);
            if (size == 0)
                return Default.EmptyArray<IViewInitializer>();
            if (size == 1)
                return managers[0].GetInitializersByViewModel(this, viewModel, metadata);

            managers = Managers.ToArrayWithLock();
            var result = new List<IViewInitializer>();
            for (var i = 0; i < managers.Length; i++)
                result.AddRange(managers[i].GetInitializersByViewModel(this, viewModel, metadata));
            return result;
        }

        protected virtual void OnViewModelCreated(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnViewModelCreated(this, viewModel, view, metadata);
        }

        protected virtual void OnViewCreated(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnViewCreated(this, viewModel, view, metadata);
        }

        protected virtual void OnViewInitialized(IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnViewInitialized(this, viewModel, viewInfo, metadata);
        }

        protected virtual void OnViewCleared(IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnViewCleared(this, viewModel, viewInfo, metadata);
        }

        #endregion
    }
}