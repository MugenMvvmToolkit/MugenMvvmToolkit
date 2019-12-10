using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class MetadataComponentExtensions
    {
        #region Methods

        public static IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(this IMetadataContextProviderComponent[] components, object? target,
            ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetReadOnlyMetadataContext(target, values);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IMetadataContext? TryGetMetadataContext(this IMetadataContextProviderComponent[] components, object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetMetadataContext(target, values);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnReadOnlyContextCreated(this IMetadataContextProviderListener[] listeners, IMetadataContextProvider provider, IReadOnlyMetadataContext metadataContext, object? target)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReadOnlyContextCreated(provider, metadataContext, target);
        }

        public static void OnContextCreated(this IMetadataContextProviderListener[] listeners, IMetadataContextProvider provider, IMetadataContext metadataContext, object? target)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnContextCreated(provider, metadataContext, target);
        }

        #endregion
    }
}