using System;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Converters
{
    public interface IGlobalValueConverter : IComponentOwner<IGlobalValueConverter>, IComponent<IMugenApplication>
    {
        object? Convert(object? value, Type targetType, object? member = null, IReadOnlyMetadataContext? metadata = null);
    }
}