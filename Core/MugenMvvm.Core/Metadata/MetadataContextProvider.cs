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
            var components = GetComponents<IMetadataContextProviderComponent>(null);
            IReadOnlyMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                result = components[i].TryGetReadOnlyMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, components);

            var listeners = GetComponents<IMetadataContextProviderListener>(null);
            for (var i = 0; i < components.Length; i++)
                listeners[i].OnReadOnlyContextCreated(this, result!, target);
            return result;
        }

        public IMetadataContext GetMetadataContext(object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default)
        {
            var components = GetComponents<IMetadataContextProviderComponent>(null);
            IMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                result = components[i].TryGetMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, components);

            var listeners = GetComponents<IMetadataContextProviderListener>(null);
            for (var i = 0; i < components.Length; i++)
                listeners[i].OnContextCreated(this, result!, target);
            return result;
        }

        #endregion
    }
}