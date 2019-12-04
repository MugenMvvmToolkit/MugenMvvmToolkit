using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IAttachedRawMemberProviderComponent : IRawMemberProviderComponent
    {
        IReadOnlyList<IMemberInfo> GetAttachedMembers(IReadOnlyMetadataContext? metadata);
    }
}