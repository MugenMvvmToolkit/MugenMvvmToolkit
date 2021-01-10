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
    public interface IMemberProviderComponent : IComponent<IMemberManager>//todo check custom indexer
    {
        ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes, IReadOnlyMetadataContext? metadata);
    }
}