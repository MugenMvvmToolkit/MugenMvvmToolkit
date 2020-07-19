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

        public const int PositionNone = -2;
        public const int PositionUnchanged = -1;

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

        public int Count => _collectionAdapter.Count;

        #endregion

        #region Implementation of interfaces

        public void AddObserver(IItemsSourceObserver observer)
        {
            _collectionAdapter.Observers.Add(observer);
        }

        public void RemoveObserver(IItemsSourceObserver observer)
        {
            _collectionAdapter.Observers.Remove(observer);
        }

        public void DestroyContent(int p0, Object p1)
        {
        }

        public Object GetContent(int position)
        {
            var item = GetItemAt(position);
            var content = (Object)_selector.SelectTemplate(_owner, item)!;
            content.BindableMembers().SetDataContext(item);
            content.BindableMembers().SetParent(item);
            return content;
        }

        public int GetContentPosition(Object content)
        {
            if (Count == 0)
                return PositionNone;
            var context = content?.BindableMembers().DataContext();
            var index = _collectionAdapter.IndexOf(content);
            if (index < 0)
                return PositionNone;
            return index;
        }

        public ICharSequence GetTitleFormatted(int position)
        {
            return null!;
        }

        public void OnPrimaryContentChanged(int p0, Object p1, Object p2)
        {
        }

        #endregion

        #region Methods

        public void SetItemsSource(IEnumerable? items)
        {
            _collectionAdapter.Attach(items);
        }

        private object? GetItemAt(int position)
        {
            return _collectionAdapter[position];
        }

        #endregion
    }
}