using System;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Convert.Internal
{
    public class TestBindingValueConverter : IBindingValueConverter
    {
        public Func<object?, Type, object?, IReadOnlyMetadataContext, object?>? Convert { get; set; }

        public Func<object?, Type, object?, IReadOnlyMetadataContext, object?>? ConvertBack { get; set; }

        object? IBindingValueConverter.Convert(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context) =>
            Convert?.Invoke(value, targetType, parameter, context);

        object? IBindingValueConverter.ConvertBack(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context) =>
            ConvertBack?.Invoke(value, targetType, parameter, context);
    }
}