using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IMemberManagerComponent : IComponent<IMemberManager>
    {
        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}