using System.Collections;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;

namespace MugenMvvm.Android.Collections
{
    public class AndroidContentItemsSourceProvider : Object, IContentItemsSourceProvider
    {
        #region Fields

        private readonly AndroidNativeBindableCollectionAdapter _collectionAdapter;
        private readonly object _owner;
        private readonly IContentTemplateSelector _selector;
        private readonly IStableIdProvider? _stableIdProvider;

        #endregion

        #region Constructors

        public AndroidContentItemsSourceProvider(object owner, IContentTemplateSelector selector, IStableIdProvider? stableIdProvider)
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

        public virtual int Count => _collectionAdapter.Count;

        public bool HasStableId => _stableIdProvider != null;

        #endregion

        #region Implementation of interfaces

        public virtual void AddObserver(IItemsSourceObserver observer)
        {
            _collectionAdapter.Observers.Add(observer);
        }

        public virtual void RemoveObserver(IItemsSourceObserver observer)
        {
            _collectionAdapter.Observers.Remove(observer);
        }

        public long GetItemId(int position)
        {
            if (_stableIdProvider == null)
                return position;
            return _stableIdProvider.GetId(GetItemAt(position));
        }

        public bool ContainsItem(long itemId)
        {
            if (_stableIdProvider == null)
                return false;

            for (var i = 0; i < _collectionAdapter.Count; i++)
            {
                if (_stableIdProvider.GetId(_collectionAdapter[i]) == itemId)
                    return true;
            }

            return false;
        }

        public virtual Object GetContent(int position)
        {
            var item = GetItemAt(position);
            var content = (Object)_selector.SelectTemplate(_owner, item)!;
            content.BindableMembers().SetDataContext(item);
            content.BindableMembers().SetParent(_owner);
            return content;
        }

        public virtual int GetContentPosition(Object content)
        {
            if (Count == 0)
                return ContentItemsSourceProvider.PositionNone;
            var index = _collectionAdapter.IndexOf(content?.BindableMembers().DataContext());
            if (index < 0)
                return ContentItemsSourceProvider.PositionNone;
            return index;
        }

        #endregion

        #region Methods

        public virtual void SetItemsSource(IEnumerable? items)
        {
            _collectionAdapter.Attach(items);
        }

        protected virtual object? GetItemAt(int position)
        {
            return _collectionAdapter[position];
        }

        #endregion
    }
}