using System;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Binding.Resources;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class ResourceComponentExtensions
    {
        #region Methods

        public static ResourceResolverResult TryGetResource(this IResourceResolverComponent[] components, IResourceResolver resourceResolver, string name, object? state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(resourceResolver, nameof(resourceResolver));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetResource(resourceResolver, name, state, metadata);
                if (value.IsResolved)
                    return value;
            }

            return default;
        }

        public static Type? TryGetType(this ITypeResolverComponent[] components, IResourceResolver resourceResolver, string name, object? state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(resourceResolver, nameof(resourceResolver));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                var type = components[i].TryGetType(resourceResolver, name, state, metadata);
                if (type != null)
                    return type;
            }

            return null;
        }

        #endregion
    }
}