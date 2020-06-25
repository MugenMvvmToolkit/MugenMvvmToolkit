using System;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class ResourceComponentExtensions
    {
        #region Methods

        public static IResourceValue? TryGetResourceValue<TState>(this IResourceResolverComponent[] components, string name, in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetResourceValue(name, state, metadata);
                if (value != null)
                    return value;
            }

            return null;
        }

        public static Type? TryGetType<TState>(this ITypeResolverComponent[] components, string name, in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                var type = components[i].TryGetType(name, state, metadata);
                if (type != null)
                    return type;
            }

            return null;
        }

        #endregion
    }
}