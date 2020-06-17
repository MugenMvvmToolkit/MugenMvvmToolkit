using System;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Converters
{
    public interface IGlobalValueConverter : IComponentOwner<IGlobalValueConverter>, IComponent<IMugenApplication>
    {
        bool TryConvert(ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata);
    }
}