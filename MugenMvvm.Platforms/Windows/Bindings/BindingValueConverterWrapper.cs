using System;
using System.Globalization;
using System.Windows.Data;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Windows.Bindings
{
    public sealed class BindingValueConverterWrapper : IBindingValueConverter
    {
        private readonly IValueConverter _valueConverter;

        public BindingValueConverterWrapper(IValueConverter valueConverter)
        {
            Should.NotBeNull(valueConverter, nameof(valueConverter));
            _valueConverter = valueConverter;
        }

        public object? Convert(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context) =>
            _valueConverter.Convert(value, targetType, parameter, CultureInfo.CurrentCulture);

        public object? ConvertBack(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context) =>
            _valueConverter.ConvertBack(value, targetType, parameter, CultureInfo.CurrentCulture);
    }
}