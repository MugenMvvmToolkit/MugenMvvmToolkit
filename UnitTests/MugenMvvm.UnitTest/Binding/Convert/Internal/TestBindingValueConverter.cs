using System;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Convert.Internal
{
    public class TestBindingValueConverter : IBindingValueConverter
    {
        #region Properties

        public Func<object?, Type, object?, IReadOnlyMetadataContext, object?>? Convert { get; set; }

        public Func<object?, Type, object?, IReadOnlyMetadataContext, object?>? ConvertBack { get; set; }

        #endregion

        #region Implementation of interfaces

        object? IBindingValueConverter.Convert(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context) => Convert?.Invoke(value, targetType, parameter, context);

        object? IBindingValueConverter.ConvertBack(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context) => ConvertBack?.Invoke(value, targetType, parameter, context);

        #endregion
    }
}