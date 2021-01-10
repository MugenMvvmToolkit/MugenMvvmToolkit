using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Members.Components
{
    public interface IMemberManagerComponent : IComponent<IMemberManager>
    {
        ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, EnumFlags<MemberType> memberTypes, EnumFlags<MemberFlags> flags, object request,
            IReadOnlyMetadataContext? metadata);
    }
}