using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Members.Internal
{
    public class TestMemberManagerComponent : IMemberManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<Type, MemberType, MemberFlags, object, Type, IReadOnlyMetadataContext?, ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>>? TryGetMembers { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> IMemberManagerComponent.TryGetMembers<TRequest>(Type type, MemberType memberTypes, MemberFlags flags, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMembers?.Invoke(type, memberTypes, flags, request!, typeof(TRequest), metadata) ?? default;
        }

        #endregion
    }
}