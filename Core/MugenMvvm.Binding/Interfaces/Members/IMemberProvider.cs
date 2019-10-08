using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    //todo add expando/dynamic objects
    public interface IMemberProvider : IComponentOwner<IMemberProvider>, IComponent<IBindingManager>
    {
        IBindingMemberInfo? GetMember(Type type, string name, BindingMemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<IBindingMemberInfo> GetMembers(Type type, string name, BindingMemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata = null);
    }
}