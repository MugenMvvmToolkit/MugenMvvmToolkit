using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IMemberProviderComponent : IComponent<IMemberManager>
    {
        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata);
    }
}