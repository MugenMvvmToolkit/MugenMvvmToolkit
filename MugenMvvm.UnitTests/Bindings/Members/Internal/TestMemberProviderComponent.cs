using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Members.Internal
{
    public class TestMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMemberManager? _memberManager;

        #endregion

        #region Constructors

        public TestMemberProviderComponent(IMemberManager? memberManager = null)
        {
            _memberManager = memberManager;
        }

        #endregion

        #region Properties

        public Func<Type, string, EnumFlags<MemberType>, IReadOnlyMetadataContext?, ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>>? TryGetMembers { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> IMemberProviderComponent.TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes, IReadOnlyMetadataContext? metadata)
        {
            _memberManager?.ShouldEqual(memberManager);
            return TryGetMembers?.Invoke(type, name, memberTypes, metadata) ?? default;
        }

        #endregion
    }
}