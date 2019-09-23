using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    //todo add expando/dynamic objects
    public interface IMemberProvider : IComponentOwner<IMemberProvider>, IComponent<IBindingManager>
    {
        IBindingMemberInfo? GetMember(Type type, string name, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<IBindingMethodInfo> GetMethods(Type type, string name, IReadOnlyMetadataContext? metadata = null);
    }
}