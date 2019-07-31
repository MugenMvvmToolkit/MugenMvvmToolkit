using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBindingValueConverterComponent : IComponent<IBindingManager>
    {
        object? Convert(object? value, Type targetType, IBindingMemberInfo? member, IReadOnlyMetadataContext? metadata);
    }
}