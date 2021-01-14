using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Members.Internal
{
    public class TestMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        private readonly IMemberManager? _memberManager;

        public TestMemberProviderComponent(IMemberManager? memberManager = null)
        {
            _memberManager = memberManager;
        }

        public Func<Type, string, EnumFlags<MemberType>, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IMemberInfo>>? TryGetMembers { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IMemberInfo> IMemberProviderComponent.TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            _memberManager?.ShouldEqual(memberManager);
            return TryGetMembers?.Invoke(type, name, memberTypes, metadata) ?? default;
        }
    }
}