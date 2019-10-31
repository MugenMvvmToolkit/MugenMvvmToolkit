using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    public sealed class MetadataContextProvider : ComponentOwnerBase<IMetadataContextProvider>, IMetadataContextProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public MetadataContextProvider(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext GetReadOnlyMetadataContext(object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default)
        {
            var list = values.List;
            var item = values.Item;
            if (list == null && item.IsEmpty)
                return Default.Metadata;

            var components = Components.GetItems();
            IReadOnlyMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                result = (components[i] as IMetadataContextProviderComponent)?.TryGetReadOnlyMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
            {
                if (list == null)
                    result = new SingleValueMetadataContext(item);
                else
                    result = new ReadOnlyMetadataContext(list);
            }

            for (var i = 0; i < components.Length; i++)
                (components[i] as IMetadataContextProviderListener)?.OnReadOnlyContextCreated(this, result!, target);
            return result!;
        }

        public IMetadataContext GetMetadataContext(object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default)
        {
            var components = Components.GetItems();
            IMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                result = (components[i] as IMetadataContextProviderComponent)?.TryGetMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                result = new MetadataContext(values);

            for (var i = 0; i < components.Length; i++)
                (components[i] as IMetadataContextProviderListener)?.OnContextCreated(this, result!, target);
            return result!;
        }

        #endregion
    }
}