using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IRawMemberProviderComponent : IComponent<IMemberProvider>
    {
        IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata);
    }
}