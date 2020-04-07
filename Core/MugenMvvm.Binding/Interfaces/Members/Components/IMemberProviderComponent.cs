using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IMemberProviderComponent : IComponent<IMemberManager>
    {
        IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata);
    }
}