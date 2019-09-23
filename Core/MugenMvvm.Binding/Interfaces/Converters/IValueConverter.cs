using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Converters
{
    public interface IValueConverter
    {
        object? Convert(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context);

        object? ConvertBack(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context);
    }
}