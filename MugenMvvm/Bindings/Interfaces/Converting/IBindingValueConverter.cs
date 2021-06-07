using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Converting
{
    public interface IBindingValueConverter
    {
        object? Convert(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context);

        object? ConvertBack(object? value, Type targetType, object? parameter, IReadOnlyMetadataContext context);
    }
}