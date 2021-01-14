using System;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Interfaces.Resources.Components;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Extensions.Components
{
    public static class ResourceComponentExtensions
    {
        public static ResourceResolverResult TryGetResource(this ItemOrArray<IResourceResolverComponent> components, IResourceResolver resourceResolver, string name, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(resourceResolver, nameof(resourceResolver));
            Should.NotBeNull(name, nameof(name));
            foreach (var c in components)
            {
                var value = c.TryGetResource(resourceResolver, name, state, metadata);
                if (value.IsResolved)
                    return value;
            }

            return default;
        }

        public static Type? TryGetType(this ItemOrArray<ITypeResolverComponent> components, IResourceResolver resourceResolver, string name, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(resourceResolver, nameof(resourceResolver));
            Should.NotBeNull(name, nameof(name));
            foreach (var c in components)
            {
                var type = c.TryGetType(resourceResolver, name, state, metadata);
                if (type != null)
                    return type;
            }

            return null;
        }
    }
}