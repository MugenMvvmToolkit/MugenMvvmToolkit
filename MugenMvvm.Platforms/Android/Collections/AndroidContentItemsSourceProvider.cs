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

        #endregion

        #region Constructors

        public AndroidContentItemsSourceProvider(object owner, IContentTemplateSelector selector)
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(selector, nameof(selector));
            _owner = owner;
            _selector = selector;
            _collectionAdapter = new AndroidNativeBindableCollectionAdapter();
        }

        #endregion

        #region Properties

        public virtual int Count => _collectionAdapter.Count;

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

        public virtual void DestroyContent(int position, Object content)
        {
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

        public virtual ICharSequence GetTitleFormatted(int position)
        {
            return null!;
        }

        public virtual void OnPrimaryContentChanged(int position, Object oldContent, Object newContent)
        {
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