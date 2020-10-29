using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Convert.Components
{
    public interface IGlobalValueConverterComponent : IComponent<IGlobalValueConverter>
    {
        bool TryConvert(IGlobalValueConverter converter, ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata);
    }
}