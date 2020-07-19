using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Binding.Interfaces.Convert.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class ConverterComponentExtensions
    {
        #region Methods

        public static bool TryConvert(this IGlobalValueConverterComponent[] components, IGlobalValueConverter converter, ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(converter, nameof(converter));
            Should.NotBeNull(targetType, nameof(targetType));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryConvert(converter, ref value, targetType, member, metadata))
                    return true;
            }

            return false;
        }

        #endregion
    }
}