using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Converters
{
    public interface IGlobalValueConverter : IComponentOwner<IGlobalValueConverter>, IComponent<IBindingManager>
    {
        object? Convert(object? value, Type targetType, object? member = null, IReadOnlyMetadataContext? metadata = null);
    }
}