using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    //todo add expando/dynamic objects
    public interface IMemberProvider : IComponentOwner<IMemberProvider>, IComponent<IBindingManager>
    {
        IMemberInfo? GetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata = null);//todo state for selector

        IReadOnlyList<IMemberInfo> GetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata = null);
    }
}