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
            var result = GetComponents<IMetadataContextProviderComponent>().TryGetReadOnlyMetadataContext(target, values);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IMetadataContextProviderComponent>(this);

            GetComponents<IMetadataContextProviderListener>().OnReadOnlyContextCreated(this, result!, target);
            return result;
        }

        public IMetadataContext GetMetadataContext(object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default)
        {
            var result = GetComponents<IMetadataContextProviderComponent>().TryGetMetadataContext(target, values);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IMetadataContextProviderComponent>(this);

            GetComponents<IMetadataContextProviderListener>().OnContextCreated(this, result!, target);
            return result;
        }

        #endregion
    }
}