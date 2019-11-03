using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Converters.Components
{
    public interface IGlobalValueConverterComponent : IComponent<IGlobalValueConverter>
    {
        bool TryConvert(ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata);
    }
}