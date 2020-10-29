using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;

namespace MugenMvvm.Android.Collections
{
    public class ContentItemsSourceProvider : ItemsSourceProviderBase<IContentTemplateSelector>, IContentItemsSourceProvider
    {
        #region Constructors

        public ContentItemsSourceProvider(object owner, IContentTemplateSelector itemTemplateSelector, IStableIdProvider? stableIdProvider, ItemsSourceBindableCollectionAdapter? collectionAdapter = null)
            : base(owner, itemTemplateSelector, stableIdProvider, collectionAdapter)
        {
        }

        #endregion

        #region Implementation of interfaces

        public virtual Object GetContent(int position)
        {
            var item = GetItemAt(position);
            var content = (Object) ItemTemplateSelector.SelectTemplate(Owner, item)!;
            content.BindableMembers().SetDataContext(item);
            content.BindableMembers().SetParent(Owner);
            return content;
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

        #endregion
    }
}