using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Convert
{
    public interface IGlobalValueConverter : IComponentOwner<IGlobalValueConverter>
    {
        bool TryConvert(ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata);
    }
}