using System;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Convert.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Convert.Internal
{
    public class TestGlobalValueConverterComponent : IGlobalValueConverterComponent, IHasPriority
    {
        private readonly IGlobalValueConverter _converter;

        public TestGlobalValueConverterComponent(IGlobalValueConverter converter)
        {
            _converter = converter;
        }

        public FuncSingleRef<object?, Type, object?, IReadOnlyMetadataContext?, bool>? TryConvert { get; set; }

        public int Priority { get; set; }

        bool IGlobalValueConverterComponent.TryConvert(IGlobalValueConverter converter, ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata)
        {
            converter.ShouldEqual(_converter);
            return TryConvert?.Invoke(ref value, targetType, member, metadata) ?? false;
        }
    }
}