using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IAttachedMemberProviderComponent : IMemberProviderComponent
    {
        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> GetAttachedMembers(IReadOnlyMetadataContext? metadata);
    }
}