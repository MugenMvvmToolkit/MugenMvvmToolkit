using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Members
{
    public class TestMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        public Func<IMemberManager, Type, string, EnumFlags<MemberType>, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IMemberInfo>>? TryGetMembers { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IMemberInfo> IMemberProviderComponent.TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata) =>
            TryGetMembers?.Invoke(memberManager, type, name, memberTypes, metadata) ?? default;
    }
}