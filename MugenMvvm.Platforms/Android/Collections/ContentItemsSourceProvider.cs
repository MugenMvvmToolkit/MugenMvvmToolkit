using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;

namespace MugenMvvm.Android.Collections
{
    public class ContentItemsSourceProvider : ItemsSourceProviderBase<IContentTemplateSelector>, IContentItemsSourceProvider
    {
        public ContentItemsSourceProvider(object owner, IContentTemplateSelector itemTemplateSelector, IStableIdProvider? stableIdProvider,
            ItemsSourceBindableCollectionAdapter? collectionAdapter = null)
            : base(owner, itemTemplateSelector, stableIdProvider, collectionAdapter)
        {
        }

        public virtual Object GetContent(int position)
        {
            var item = GetItemAt(position);
            if (!ItemTemplateSelector.TrySelectTemplate(Owner, item, out var content) || content == null)
                ExceptionManager.ThrowTemplateNotSupported(Owner, item);

            content.BindableMembers().SetDataContext(item);
            content.BindableMembers().SetParent(Owner);
            return (Object) content;
        }

        public virtual int GetContentPosition(Object? content)
        {
            if (Count == 0)
                return Native.Interfaces.ContentItemsSourceProvider.PositionNone;
            var index = IndexOf(content?.BindableMembers().DataContext());
            if (index < 0)
                return Native.Interfaces.ContentItemsSourceProvider.PositionNone;
            return index;
        }
    }
}