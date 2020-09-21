using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class AttachedMemberProvider : AttachableComponentBase<IMemberManager>, IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly Dictionary<string, List<IMemberInfo>> _registeredMembers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedMemberProvider()
        {
            _registeredMembers = new Dictionary<string, List<IMemberInfo>>(59, StringComparer.Ordinal);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Attached;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            if (!_registeredMembers.TryGetValue(name, out var members))
                return default;

            var result = ItemOrListEditor.Get<IMemberInfo>();
            for (var index = 0; index < members.Count; index++)
            {
                var memberInfo = members[index];
                if (memberTypes.HasFlagEx(memberInfo.MemberType) && memberInfo.DeclaringType.IsAssignableFromGeneric(type))
                    result.Add(memberInfo);
            }

            return result.ToItemOrList<IReadOnlyList<IMemberInfo>>();
        }

        #endregion

        #region Methods

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> GetAttachedMembers(IReadOnlyMetadataContext? metadata)
        {
            var members = ItemOrListEditor.Get<IMemberInfo>();
            foreach (var keyValuePair in _registeredMembers)
                members.AddRange(ItemOrList.FromList(keyValuePair.Value));
            return members.ToItemOrList<IReadOnlyList<IMemberInfo>>();
        }

        public void Register(IMemberInfo member, string? name = null)
        {
            Should.NotBeNull(member, nameof(member));
            name ??= member.Name;
            if (!_registeredMembers.TryGetValue(name, out var list))
            {
                list = new List<IMemberInfo>();
                _registeredMembers[name] = list;
            }

            list.Add(member);
            OwnerOptional.TryInvalidateCache();
        }

        public void Unregister(IMemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var removed = false;
            foreach (var pair in _registeredMembers)
            {
                if (pair.Value.Remove(member))
                    removed = true;
            }

            if (removed)
                OwnerOptional.TryInvalidateCache();
        }

        public void Clear()
        {
            _registeredMembers.Clear();
            OwnerOptional.TryInvalidateCache();
        }

        #endregion
    }
}