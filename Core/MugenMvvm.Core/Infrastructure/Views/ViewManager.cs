using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        #region Fields

        private readonly ChildManagersCollection _viewManagers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManager()
        {
            _viewManagers = new ChildManagersCollection(this);
        }

        #endregion

        #region Properties

        public ICollection<IChildViewManager> ViewManagers => _viewManagers;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewInfo> GetViews(IViewModel viewModel, IReadOnlyMetadataContext metadata)
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

        public IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetInitializersByViewModelInternal(viewModel, metadata);
        }

        void IParentViewManager.OnViewInitialized(IViewModel viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            OnViewInitialized(viewModel, view, metadata);
        }

        void IParentViewManager.OnViewCleared(IViewModel viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            OnViewCleared(viewModel, view, metadata);
        }

        #endregion

        #region Methods

        protected virtual IReadOnlyList<IViewInfo> GetViewsInternal(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            if (_viewManagers.Count == 0)
                return Default.EmptyArray<IViewInfo>();
            if (_viewManagers.Count == 1)
                return _viewManagers.GetAt(0).GetViews(this, viewModel, metadata);

            var managers = _viewManagers.ToArray();
            var result = new List<IViewInfo>();
            for (var i = 0; i < managers.Length; i++)
                result.AddRange(managers[i].GetViews(this, viewModel, metadata));
            return result;
        }

        protected virtual IReadOnlyList<IViewModelViewInitializer> GetInitializersByViewInternal(object view, IReadOnlyMetadataContext metadata)
        {
            if (_viewManagers.Count == 0)
                return Default.EmptyArray<IViewModelViewInitializer>();
            if (_viewManagers.Count == 1)
                return _viewManagers.GetAt(0).GetInitializersByView(this, view, metadata);

            var managers = _viewManagers.ToArray();
            var result = new List<IViewModelViewInitializer>();
            for (var i = 0; i < managers.Length; i++)
                result.AddRange(managers[i].GetInitializersByView(this, view, metadata));
            return result;
        }

        protected virtual IReadOnlyList<IViewInitializer> GetInitializersByViewModelInternal(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            if (_viewManagers.Count == 0)
                return Default.EmptyArray<IViewInitializer>();
            if (_viewManagers.Count == 1)
                return _viewManagers.GetAt(0).GetInitializersByViewModel(this, viewModel, metadata);

            var managers = _viewManagers.ToArray();
            var result = new List<IViewInitializer>();
            for (var i = 0; i < managers.Length; i++)
                result.AddRange(managers[i].GetInitializersByViewModel(this, viewModel, metadata));
            return result;
        }

        protected virtual void OnViewInitialized(IViewModel viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnViewInitialized(this, viewModel, view, metadata);
        }

        protected virtual void OnViewCleared(IViewModel viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnViewCleared(this, viewModel, view, metadata);
        }

        protected virtual void OnChildViewManagerAdded(IChildViewManager childViewManager)
        {
        }

        protected virtual void OnChildViewManagerRemoved(IChildViewManager childViewManager)
        {
        }

        #endregion

        #region Nested types

        private sealed class ChildManagersCollection : ICollection<IChildViewManager>, IComparer<IChildViewManager>
        {
            #region Fields

            private readonly OrderedListInternal<IChildViewManager> _list;
            private readonly ViewManager _manager;

            #endregion

            #region Constructors

            public ChildManagersCollection(ViewManager manager)
            {
                _manager = manager;
                _list = new OrderedListInternal<IChildViewManager>(comparer: this);
            }

            #endregion

            #region Properties

            public int Count => _list.Count;

            public bool IsReadOnly => false;

            #endregion

            #region Implementation of interfaces

            public IEnumerator<IChildViewManager> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(IChildViewManager item)
            {
                Should.NotBeNull(item, nameof(item));
                _list.Add(item);
                _manager.OnChildViewManagerAdded(item);
            }

            public void Clear()
            {
                var values = _list.ToArray();
                _list.Clear();
                for (var index = 0; index < values.Length; index++)
                    _manager.OnChildViewManagerRemoved(values[index]);
            }

            public bool Contains(IChildViewManager item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(IChildViewManager[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(IChildViewManager item)
            {
                Should.NotBeNull(item, nameof(item));
                var remove = _list.Remove(item);
                if (remove)
                    _manager.OnChildViewManagerRemoved(item);
                return remove;
            }

            public int Compare(IChildViewManager x1, IChildViewManager x2)
            {
                return x2.Priority.CompareTo(x1.Priority);
            }

            #endregion

            #region Methods

            public IChildViewManager GetAt(int index)
            {
                return _list[index];
            }

            #endregion
        }

        #endregion
    }
}