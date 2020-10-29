using Android.Views;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;

namespace MugenMvvm.Android.Collections
{
    public class ResourceItemsSourceProvider : ItemsSourceProviderBase<IResourceTemplateSelector>, IResourceItemsSourceProvider
    {
        #region Constructors

        public ResourceItemsSourceProvider(object owner, IResourceTemplateSelector itemTemplateSelector, IStableIdProvider? stableIdProvider, ItemsSourceBindableCollectionAdapter? collectionAdapter = null)
            : base(owner, itemTemplateSelector, stableIdProvider, collectionAdapter)
        {
        }

        #endregion

        #region Properties

        public virtual int ViewTypeCount => ItemTemplateSelector.TemplateTypeCount;

        #endregion

        #region Implementation of interfaces

        public virtual int GetItemViewType(int position) => ItemTemplateSelector.SelectTemplate(Owner, GetItemAt(position));

        public virtual void OnBindView(View view, int position) => view.BindableMembers().SetDataContext(GetItemAt(position));

        public virtual void OnViewCreated(View view)
        {
            view.BindableMembers().SetDataContext(null);
            view.BindableMembers().SetParent(view);
        }

        #endregion
    }
}