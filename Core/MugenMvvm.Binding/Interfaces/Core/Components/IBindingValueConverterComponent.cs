using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingValueConverterComponent : IComponent<IBindingManager>//todo check move to resources
    {
        object? Convert(object? value, Type targetType, IBindingMemberInfo? member, IReadOnlyMetadataContext? metadata);
    }
}