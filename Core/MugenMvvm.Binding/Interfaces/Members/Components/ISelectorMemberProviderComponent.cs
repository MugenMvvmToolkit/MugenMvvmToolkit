using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface ISelectorMemberProviderComponent : IComponent<IMemberProvider>
    {
        IMemberInfo? TryGetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata);
    }
}