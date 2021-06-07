using System;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Bindings.Interfaces.Converting.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Converting
{
    public class TestGlobalValueConverterComponent : IGlobalValueConverterComponent, IHasPriority
    {
        public TryConvertDelegate? TryConvert { get; set; }

        public int Priority { get; set; }

        bool IGlobalValueConverterComponent.TryConvert(IGlobalValueConverter converter, ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata) =>
            TryConvert?.Invoke(converter, ref value, targetType, member, metadata) ?? false;

        public delegate bool TryConvertDelegate(IGlobalValueConverter converter, ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata);
    }
}