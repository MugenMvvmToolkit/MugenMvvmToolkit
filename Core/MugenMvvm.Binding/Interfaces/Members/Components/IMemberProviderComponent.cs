using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IMemberProviderComponent : IComponent<IMemberProvider>
    {
        IBindingMemberInfo? TryGetMember(Type type, string name, IReadOnlyMetadataContext? metadata);
    }
}