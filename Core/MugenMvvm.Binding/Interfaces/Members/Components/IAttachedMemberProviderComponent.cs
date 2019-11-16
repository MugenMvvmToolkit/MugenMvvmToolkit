using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IAttachedMemberProviderComponent : IMemberProviderComponent
    {
        IReadOnlyList<IMemberInfo> GetAttachedMembers(IReadOnlyMetadataContext? metadata);
    }
}