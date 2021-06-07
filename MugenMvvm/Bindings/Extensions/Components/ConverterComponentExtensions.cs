using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Bindings.Interfaces.Converting.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Extensions.Components
{
    public static class ConverterComponentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryConvert(this ItemOrArray<IGlobalValueConverterComponent> components, IGlobalValueConverter converter, ref object? value, Type targetType,
            object? member, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(converter, nameof(converter));
            Should.NotBeNull(targetType, nameof(targetType));
            foreach (var c in components)
            {
                if (c.TryConvert(converter, ref value, targetType, member, metadata))
                    return true;
            }

            return false;
        }
    }
}