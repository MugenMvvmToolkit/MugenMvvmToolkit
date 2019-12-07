using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class SelectorMemberProviderComponent : ISelectorMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly SelectorDictionary _selectorDictionary;

        #endregion

        #region Constructors

        public SelectorMemberProviderComponent()
        {
            _selectorDictionary = new SelectorDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Selector;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IMemberInfo>? TrySelectMembers(IReadOnlyList<IMemberInfo> members, Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            _selectorDictionary.Clear();
            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var memberType = member.MemberType;
                if (!memberTypes.HasFlagEx(memberType) || !flags.HasFlagEx(member.AccessModifiers))
                    continue;

                if (!_selectorDictionary.TryGetValue(member, out var currentMember) || GetPriority(member) > GetPriority(currentMember))
                    _selectorDictionary[member] = member;
            }

            return _selectorDictionary.ValuesToArray();
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
                if (key is IMethodInfo method)
                    return HashCode.Combine((byte)key.MemberType, method.GetParameters().Count);
                return HashCode.Combine((byte)key.MemberType);
            }

            protected override bool Equals(IMemberInfo x, IMemberInfo y)
            {
                if (x.MemberType != y.MemberType)
                    return false;

                if (x.MemberType != MemberType.Method)
                    return true;

                var xM = ((IMethodInfo)x).GetParameters();
                var yM = ((IMethodInfo)y).GetParameters();
                if (xM.Count != yM.Count)
                    return false;

                for (var i = 0; i < xM.Count; i++)
                {
                    if (xM[i].ParameterType != yM[i].ParameterType)
                        return false;
                }

                return true;
            }

            #endregion
        }

        #endregion
    }
}