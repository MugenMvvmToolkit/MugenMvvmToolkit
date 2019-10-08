using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IMemberProviderComponent : IComponent<IMemberProvider>
    {
        IReadOnlyList<IBindingMemberInfo> TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata);
    }
}