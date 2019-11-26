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
            var components = Components.GetComponents();
            IReadOnlyMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                result = (components[i] as IMetadataContextProviderComponent)?.TryGetReadOnlyMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMetadataContextProviderComponent).Name);

            for (var i = 0; i < components.Length; i++)
                (components[i] as IMetadataContextProviderListener)?.OnReadOnlyContextCreated(this, result!, target);
            return result;
        }

        public IMetadataContext GetMetadataContext(object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default)
        {
            var components = Components.GetComponents();
            IMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                result = (components[i] as IMetadataContextProviderComponent)?.TryGetMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMetadataContextProviderComponent).Name);

            for (var i = 0; i < components.Length; i++)
                (components[i] as IMetadataContextProviderListener)?.OnContextCreated(this, result!, target);
            return result;
        }

        #endregion
    }
}