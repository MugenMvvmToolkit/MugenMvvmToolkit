using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Converters
{
    public interface IGlobalBindingValueConverter : IComponentOwner<IGlobalBindingValueConverter>, IComponent<IBindingManager>
    {
        object? Convert(object? value, Type targetType, IBindingMemberInfo? member = null, IReadOnlyMetadataContext? metadata = null);
    }
}