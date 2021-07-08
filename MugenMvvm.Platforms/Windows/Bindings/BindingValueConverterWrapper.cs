using System;
using System.Globalization;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Interfaces.Metadata;

#if AVALONIA
using Avalonia.Data.Converters;

namespace MugenMvvm.Avalonia.Bindings
#else
using System.Windows.Data;

namespace MugenMvvm.Windows.Bindings
#endif
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