using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class MethodMemberAccessorDecorator : ComponentDecoratorBase<IMemberManager, IMemberProviderComponent>, IMemberProviderComponent
    {
        #region Fields

        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly List<IMemberInfo> _members;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MethodMemberAccessorDecorator(IGlobalValueConverter? globalValueConverter = null, int priority = MemberComponentPriority.MethodAccessorDecorator)
            : base(priority)
        {
            _globalValueConverter = globalValueConverter;
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes, IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlag(MemberType.Accessor))
                return Components.TryGetMembers(memberManager, type, name, memberTypes, metadata);

            var methodArgsRaw = BindingMugenExtensions.GetMethodArgsRaw(name, out var methodName);
            if (methodArgsRaw.IsEmpty)
                return Components.TryGetMembers(memberManager, type, name, memberTypes, metadata);

            _members.Clear();
            Components.TryAddMembers(memberManager, _members, type, methodName, MemberType.Method, metadata);
            for (var i = 0; i < _members.Count; i++)
            {
                if (_members[i] is IMethodMemberInfo methodInfo)
                {
                    var values = _globalValueConverter.TryGetInvokeArgs(methodInfo.GetParameters(), methodArgsRaw, metadata, out var flags);
                    if (!values.IsEmpty)
                    {
                        _members[i] = methodInfo.TryGetAccessor(flags, values, metadata) ?? new MethodAccessorMemberInfo(methodName, methodInfo, null, values, flags, type);
                        continue;
                    }
                }

                _members.RemoveAt(i);
                --i;
            }

            Components.TryAddMembers(memberManager, _members, type, name, memberTypes, metadata);
            return _members.ToItemOrList(true);
        }

        #endregion
    }
}