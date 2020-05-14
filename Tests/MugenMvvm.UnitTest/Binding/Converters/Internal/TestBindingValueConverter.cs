using System;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Converters.Internal
{
    public class TestBindingValueConverter : IBindingValueConverter
    {
        #region Properties

        public Func<object?, Type, object?, IReadOnlyMetadataContext, object?>? Convert { get; set; }

        public Func<object?, Type, object?, IReadOnlyMetadataContext, object?>? ConvertBack { get; set; }

        #endregion

        #region Implementation of interfaces

        object? IBindingValueConverter.Convert(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context)
        {
            return Convert?.Invoke(value, targetType, parameter, context);
        }

        object? IBindingValueConverter.ConvertBack(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context)
        {
            return ConvertBack?.Invoke(value, targetType, parameter, context);
        }

        #endregion
    }
}