using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class MemberSelector : IMemberManagerComponent
    {
        #region Fields

        private readonly SelectorDictionary _selectorDictionary;

        private const int AttachedPriority = 1000000;
        private const int InstancePriority = 100000;
        private const int ExtensionPriority = 10000;
        private const int DynamicPriority = 1000;
        private const int MaxDeclaringTypePriority = 100;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberSelector()
        {
            _selectorDictionary = new SelectorDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Selector;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(Type type, MemberType memberTypes, MemberFlags flags, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TRequest>() || !(request is IReadOnlyList<IMemberInfo> members))
                return default;

            _selectorDictionary.Clear();
            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var memberType = member.MemberType;
                if (!memberTypes.HasFlagEx(memberType) || !flags.HasFlagEx(member.AccessModifiers))
                    continue;

                if (!_selectorDictionary.TryGetValue(member, out var currentMember) || GetPriority(member, type) > GetPriority(currentMember, type))
                    _selectorDictionary[member] = member;
            }

            if (_selectorDictionary.Count == 0)
                return default;
            if (_selectorDictionary.Count == 1)
                return new ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>(_selectorDictionary.FirstOrDefault.Value);
            return _selectorDictionary.ValuesToArray();
        }

        #endregion

        #region Methods

        private static int GetPriority(IMemberInfo member, Type requestedType)
        {
            var priority = 0;
            if (requestedType == member.DeclaringType)
                priority = MaxDeclaringTypePriority;
            else if (!requestedType.IsInterface)
            {
                if (member.DeclaringType.IsInterface)
                    priority = 1;
                else
                {
                    var type = requestedType;
                    var nestedCount = 0;
                    while (type != null)
                    {
                        type = type.BaseType;
                        if (type == member.DeclaringType)
                        {
                            priority = MaxDeclaringTypePriority - nestedCount;
                            break;
                        }

                        ++nestedCount;
                    }
                }
            }

            priority += GetArgsPriority(member);

            if (member.AccessModifiers.HasFlagEx(MemberFlags.Attached))
                return AttachedPriority + priority;
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Extension))
                return ExtensionPriority + priority;
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Dynamic))
                return DynamicPriority + priority;
            return InstancePriority + priority;
        }

        private static int GetArgsPriority(IMemberInfo member)
        {
            if (!(member is IHasArgsMemberInfo hasArgs))
                return 0;
            switch (hasArgs.ArgumentFlags)
            {
                case ArgumentFlags.Metadata:
                    return -1;
                case ArgumentFlags.Optional:
                    return -2;
                case ArgumentFlags.ParamArray:
                    return -3;
                case ArgumentFlags.EmptyParamArray:
                    return -4;
                default:
                    return 0;
            }
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
                if (key is IMethodMemberInfo method)
                    return HashCode.Combine((byte) key.MemberType, method.GetParameters().Count);
                return HashCode.Combine((byte) key.MemberType);
            }

            protected override bool Equals(IMemberInfo x, IMemberInfo y)
            {
                if (x.MemberType != y.MemberType)
                    return false;

                if (x.MemberType != MemberType.Method)
                    return true;

                var xM = ((IMethodMemberInfo) x).GetParameters();
                var yM = ((IMethodMemberInfo) y).GetParameters();
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