using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class TestMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        #region Properties

        public Func<Type, string, IReadOnlyMetadataContext?, ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>>? TryGetMembers { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> IMemberProviderComponent.TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMembers?.Invoke(type, name, metadata) ?? default;
        }

        #endregion
    }
}