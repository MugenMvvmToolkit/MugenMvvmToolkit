using System;
using MugenMvvm.Binding.Interfaces.Convert.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Convert.Internal
{
    public class TestGlobalValueConverterComponent : IGlobalValueConverterComponent, IHasPriority
    {
        #region Properties

        public FuncSingleRef<object?, Type, object?, IReadOnlyMetadataContext?, bool>? TryConvert { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IGlobalValueConverterComponent.TryConvert(ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata)
        {
            return TryConvert?.Invoke(ref value, targetType, member, metadata) ?? false;
        }

        #endregion
    }
}