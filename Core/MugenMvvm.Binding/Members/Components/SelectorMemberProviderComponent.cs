using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class SelectorMemberProviderComponent : ComponentTrackerBase<IMemberProvider, IMemberProviderComponent>, ISelectorMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly SelectorDictionary _dictionary;

        #endregion

        #region Constructors

        public SelectorMemberProviderComponent()
        {
            _dictionary = new SelectorDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Selector;

        #endregion

        #region Implementation of interfaces

        public IMemberInfo? TryGetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            var currentPriority = int.MinValue;
            IMemberInfo? currentMember = null;
            var providers = Components;
            for (var i = 0; i < providers.Length; i++)
            {
                var members = providers[i].TryGetMembers(type, name, metadata);
                for (var j = 0; j < members.Count; j++)
                {
                    var member = members[j];
                    if (!memberTypes.HasFlagEx(member.MemberType) || !flags.HasFlagEx(member.AccessModifiers))
                        continue;

                    var priority = GetPriority(member);
                    if (priority < currentPriority)
                        continue;
                    if (priority == currentPriority)
                    {
                        if (memberTypes.HasFlagEx(MemberType.Method))
                            throw new AmbiguousMatchException();
                        continue;
                    }

                    currentPriority = priority;
                    currentMember = member;
                }
            }

            return currentMember;
        }

        public IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            _dictionary.Clear();
            var providers = Components;
            for (var i = 0; i < providers.Length; i++)
            {
                var members = providers[i].TryGetMembers(type, name, metadata);
                for (var j = 0; j < members.Count; j++)
                {
                    var member = members[j];
                    if (!memberTypes.HasFlagEx(member.MemberType) || !flags.HasFlagEx(member.AccessModifiers))
                        continue;

                    if (!_dictionary.TryGetValue(member, out var currentMember) || GetPriority(member) > GetPriority(currentMember))
                        _dictionary[member] = member;
                }
            }

            return _dictionary.KeysToArray();
        }

        #endregion

        #region Methods

        private static int GetPriority(IMemberInfo member)
        {
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Attached))
                return MemberComponentPriority.Attached;
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Extension))
                return MemberComponentPriority.Extension;
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Dynamic))
                return MemberComponentPriority.Dynamic;
            return MemberComponentPriority.Reflection;
        }

        #endregion

        #region Nested types

        private sealed class SelectorDictionary : LightDictionary<IMemberInfo, IMemberInfo>
        {
            #region Constructors

            public SelectorDictionary() : base(17)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(IMemberInfo key)
            {
                return key.GetHashCodeEx();
            }

            protected override bool Equals(IMemberInfo x, IMemberInfo y)
            {
                return x.EqualsEx(y);
            }

            #endregion
        }

        #endregion
    }
}