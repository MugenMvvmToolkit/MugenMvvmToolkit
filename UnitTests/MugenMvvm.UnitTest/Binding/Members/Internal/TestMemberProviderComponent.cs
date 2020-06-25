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
    public class TestMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        #region Properties

        public Func<Type, string, MemberType, IReadOnlyMetadataContext?, ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>>? TryGetMembers { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> IMemberProviderComponent.TryGetMembers(Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMembers?.Invoke(type, name, memberTypes, metadata) ?? default;
        }

        #endregion
    }
}