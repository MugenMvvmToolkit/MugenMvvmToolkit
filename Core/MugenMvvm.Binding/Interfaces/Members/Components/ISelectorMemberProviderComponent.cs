using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface ISelectorMemberProviderComponent
    {
        IReadOnlyList<IMemberInfo>? TrySelectMembers(IReadOnlyList<IMemberInfo> members, Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata);
    }
}