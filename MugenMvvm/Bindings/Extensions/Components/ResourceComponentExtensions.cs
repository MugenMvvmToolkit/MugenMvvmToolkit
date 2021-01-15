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
        public static ResourceResolverResult TryGetResource(this ItemOrArray<IResourceResolverComponent> components, IResourceManager resourceManager, string name, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(resourceManager, nameof(resourceManager));
            Should.NotBeNull(name, nameof(name));
            foreach (var c in components)
            {
                var value = c.TryGetResource(resourceManager, name, state, metadata);
                if (value.IsResolved)
                    return value;
            }

            return default;
        }

        public static Type? TryGetType(this ItemOrArray<ITypeResolverComponent> components, IResourceManager resourceManager, string name, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(resourceManager, nameof(resourceManager));
            Should.NotBeNull(name, nameof(name));
            foreach (var c in components)
            {
                var type = c.TryGetType(resourceManager, name, state, metadata);
                if (type != null)
                    return type;
            }

            return null;
        }
    }
}