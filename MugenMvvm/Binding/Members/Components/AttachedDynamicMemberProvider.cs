using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class AttachedDynamicMemberProvider : AttachableComponentBase<IMemberManager>, IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly List<Func<Type, string, MemberType, IReadOnlyMetadataContext?, IMemberInfo?>> _dynamicMembers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedDynamicMemberProvider()
        {
            _dynamicMembers = new List<Func<Type, string, MemberType, IReadOnlyMetadataContext?, IMemberInfo?>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Attached;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            if (_dynamicMembers.Count == 0)
                return default;
            ItemOrList<IMemberInfo, List<IMemberInfo>> members = default;
            for (var i = 0; i < _dynamicMembers.Count; i++)
                members.Add(_dynamicMembers[i].Invoke(type, name, memberTypes, metadata));
            return members.Cast<IReadOnlyList<IMemberInfo>>();
        }

        #endregion

        #region Methods

        public void Register(Func<Type, string, MemberType, IReadOnlyMetadataContext?, IMemberInfo?> getMember)
        {
            Should.NotBeNull(getMember, nameof(getMember));
            _dynamicMembers.Add(getMember);
            OwnerOptional.TryInvalidateCache();
        }

        public void Unregister(Func<Type, string, MemberType, IReadOnlyMetadataContext?, IMemberInfo?> getMember)
        {
            Should.NotBeNull(getMember, nameof(getMember));
            if (_dynamicMembers.Remove(getMember))
                OwnerOptional.TryInvalidateCache();
        }

        public void Clear()
        {
            _dynamicMembers.Clear();
            OwnerOptional.TryInvalidateCache();
        }

        #endregion
    }
}