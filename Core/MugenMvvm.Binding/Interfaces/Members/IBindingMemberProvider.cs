using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    //todo add expando/dynamic objects
    public interface IBindingMemberProvider : IComponentOwner<IBindingMemberProvider>, IComponent<IBindingManager>
    {
        IBindingMemberInfo? GetMember(Type type, string name, IReadOnlyMetadataContext? metadata = null);

        IBindingMemberInfo? GetRawMember(Type type, string name, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<AttachedMemberRegistration> GetAttachedMembers(Type type, IReadOnlyMetadataContext? metadata = null);
    }

    //        void Register(Type type, IBindingMemberInfo member, string? name, IReadOnlyMetadataContext? metadata = null);

    //bool Unregister(Type type, string? name, IReadOnlyMetadataContext? metadata = null);
}