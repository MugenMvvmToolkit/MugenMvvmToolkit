using System;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Binding.Interfaces.Convert.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Binding.Convert.Internal
{
    public class TestGlobalValueConverterComponent : IGlobalValueConverterComponent, IHasPriority
    {
        #region Fields

        private readonly IGlobalValueConverter _converter;

        #endregion

        #region Constructors

        public TestGlobalValueConverterComponent(IGlobalValueConverter converter)
        {
            _converter = converter;
        }

        #endregion

        #region Properties

        public FuncSingleRef<object?, Type, object?, IReadOnlyMetadataContext?, bool>? TryConvert { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Methods

        bool IGlobalValueConverterComponent.TryConvert(IGlobalValueConverter converter, ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata)
        {
            converter.ShouldEqual(_converter);
            return TryConvert?.Invoke(ref value, targetType, member, metadata) ?? false;
        }

        #endregion
    }
}