using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Converters.Components
{
    public interface IGlobalValueConverterComponent : IComponent<IGlobalValueConverter>
    {
        object? Convert(object? value, Type targetType, IBindingMemberInfo? member, IReadOnlyMetadataContext? metadata);
    }
}