using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Converting
{
    public interface IGlobalValueConverter : IComponentOwner<IGlobalValueConverter>
    {
        bool TryConvert(ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata);
    }
}