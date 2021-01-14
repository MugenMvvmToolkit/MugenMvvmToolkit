using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IMemberManager : IComponentOwner<IMemberManager>
    {
        ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(Type type, EnumFlags<MemberType> memberTypes, EnumFlags<MemberFlags> flags, object request,
            IReadOnlyMetadataContext? metadata = null);
    }
}