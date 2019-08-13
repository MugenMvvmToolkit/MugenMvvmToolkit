using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IAttachedBindingMemberProviderComponent : IComponent<IBindingMemberProvider>
    {
        IBindingMemberInfo? TryGetMember(Type type, string name, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<AttachedMemberRegistration> GetMembers(Type type, IReadOnlyMetadataContext? metadata);
    }
}