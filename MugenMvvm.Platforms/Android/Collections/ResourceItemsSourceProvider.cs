using Android.Views;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;

namespace MugenMvvm.Android.Collections
{
    public class ResourceItemsSourceProvider : ItemsSourceProviderBase<IResourceTemplateSelector>, IResourceItemsSourceProvider
    {
        public ResourceItemsSourceProvider(object owner, IResourceTemplateSelector itemTemplateSelector, IStableIdProvider? stableIdProvider,
            ItemsSourceBindableCollectionAdapter? collectionAdapter = null)
            : base(owner, itemTemplateSelector, stableIdProvider, collectionAdapter)
        {
        }

        public virtual int ViewTypeCount => ItemTemplateSelector.TemplateTypeCount;

        public virtual int GetItemViewType(int position)
        {
            var id = ItemTemplateSelector.TrySelectTemplate(Owner, GetItemAt(position));
            if (id == IResourceTemplateSelector.NoResult)
                ExceptionManager.ThrowTemplateNotSupported(Owner, GetItemAt(position));
            return id;
        }

        public virtual void OnBindView(View view, int position) => view.BindableMembers().SetDataContext(GetItemAt(position));

        public virtual void OnViewCreated(View view)
        {
            view.BindableMembers().SetDataContext(null);
            view.BindableMembers().SetParent(Owner);
        }
    }
}