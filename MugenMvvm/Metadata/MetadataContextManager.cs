using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    public sealed class MetadataContextManager : ComponentOwnerBase<IMetadataContextManager>, IMetadataContextManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public MetadataContextManager(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default)
        {
            var result = GetComponents<IMetadataContextProviderComponent>().TryGetReadOnlyMetadataContext(target, values);
            if (result != null)
                GetComponents<IMetadataContextManagerListener>().OnReadOnlyContextCreated(this, result!, target);
            return result;
        }

        public IMetadataContext? TryGetMetadataContext(object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default)
        {
            var result = GetComponents<IMetadataContextProviderComponent>().TryGetMetadataContext(target, values);
            if (result != null)
                GetComponents<IMetadataContextManagerListener>().OnContextCreated(this, result!, target);
            return result;
        }

        #endregion
    }
}