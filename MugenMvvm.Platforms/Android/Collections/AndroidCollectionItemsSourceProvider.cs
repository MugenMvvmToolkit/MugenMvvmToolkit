using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;

namespace MugenMvvm.Android.Collections
{
    public sealed class AndroidCollectionItemsSourceProvider : Object, IItemsSourceProvider
    {
        #region Fields

        private readonly AndroidNativeBindableCollectionAdapter _collectionAdapter;
        private readonly object _owner;
        private readonly IDataTemplateSelector _selector;
        private readonly IStableIdProvider? _stableIdProvider;

        #endregion

        #region Constructors

        public AndroidCollectionItemsSourceProvider(object owner, IDataTemplateSelector selector, IStableIdProvider? stableIdProvider)
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(selector, nameof(selector));
            _owner = owner;
            _selector = selector;
            _stableIdProvider = stableIdProvider;
            _collectionAdapter = new AndroidNativeBindableCollectionAdapter();
        }

        #endregion

        #region Properties

        public int Count => _collectionAdapter.Count;

        public bool HasStableId => _stableIdProvider != null;

        public int ViewTypeCount => _selector.TemplateTypeCount;

        #endregion

        #region Implementation of interfaces

        public long GetItemId(int position)
        {
            if (_stableIdProvider == null)
                return 0;
            return _stableIdProvider.GetId(GetItemAt(position));
        }

        public int GetItemResourceId(int position)
        {
            return _selector.SelectTemplate(GetItemAt(position), _owner);
        }

        public void OnBindView(Object owner, Object view, int position)
        {
            view.BindableMembers().SetDataContext(GetItemAt(position));
        }

        public void OnViewCreated(Object owner, Object view)
        {
            view.BindableMembers().SetDataContext(null);
        }

        public void AddObserver(IItemsSourceObserver observer)
        {
            _collectionAdapter.Observers.Add(observer);
        }

        public void RemoveObserver(IItemsSourceObserver observer)
        {
            _collectionAdapter.Observers.Remove(observer);
        }

        #endregion

        #region Methods

        public void SetItemsSource(IEnumerable? items)
        {
            _collectionAdapter.Attach(items as IEnumerable<object?> ?? items?.Cast<object?>());
        }

        private object? GetItemAt(int position)
        {
            return _collectionAdapter[position];
        }

        #endregion
    }
}