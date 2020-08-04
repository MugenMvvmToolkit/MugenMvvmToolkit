using System.Collections;
using Android.Views;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;

namespace MugenMvvm.Android.Collections
{
    public class AndroidResourceItemsSourceProvider : Object, IResourceItemsSourceProvider
    {
        #region Fields

        private readonly AndroidNativeBindableCollectionAdapter _collectionAdapter;
        private readonly object _owner;
        private readonly IDataTemplateSelector _selector;
        private readonly IStableIdProvider? _stableIdProvider;

        #endregion

        #region Constructors

        public AndroidResourceItemsSourceProvider(object owner, IDataTemplateSelector selector, IStableIdProvider? stableIdProvider)
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

        public virtual bool HasStableId => _stableIdProvider != null;

        public virtual int ViewTypeCount => _selector.TemplateTypeCount;

        #endregion

        #region Implementation of interfaces

        public virtual long GetItemId(int position)
        {
            if (_stableIdProvider == null)
                return position;
            return _stableIdProvider.GetId(GetItemAt(position));
        }

        public virtual int GetItemViewType(int position)
        {
            return _selector.SelectTemplate(_owner, GetItemAt(position));
        }

        public virtual void OnBindView(View view, int position)
        {
            view.BindableMembers().SetDataContext(GetItemAt(position));
        }

        public virtual void OnViewCreated(View view)
        {
            view.BindableMembers().SetDataContext(null);
            view.BindableMembers().SetParent(view);
        }

        public virtual void AddObserver(IItemsSourceObserver observer)
        {
            _collectionAdapter.Observers.Add(observer);
        }

        public virtual void RemoveObserver(IItemsSourceObserver observer)
        {
            _collectionAdapter.Observers.Remove(observer);
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