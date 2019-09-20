using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IBindingMemberProviderComponent : IComponent<IBindingMemberProvider>
    {
        IBindingMemberInfo? TryGetMember(Type type, string name, IReadOnlyMetadataContext? metadata);
    }
}