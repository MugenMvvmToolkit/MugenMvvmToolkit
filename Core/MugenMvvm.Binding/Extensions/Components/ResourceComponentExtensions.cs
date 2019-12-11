using System;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class ResourceComponentExtensions
    {
        #region Methods

        public static IResourceValue? TryGetResourceValue(this IResourceResolverComponent[] components, string name, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetResourceValue(name, metadata);
                if (value != null)
                    return value;
            }

            return null;
        }

        public static IBindingValueConverter? TryGetConverter(this IBindingValueConverterResolverComponent[] components, string name, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var converter = components[i].TryGetConverter(name, metadata);
                if (converter != null)
                    return converter;
            }

            return null;
        }

        public static Type? TryGetType(this ITypeResolverComponent[] components, string name, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var type = components[i].TryGetType(name, metadata);
                if (type != null)
                    return type;
            }

            return null;
        }

        #endregion
    }
}