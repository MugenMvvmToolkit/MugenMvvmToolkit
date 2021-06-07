using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class MethodMemberAccessorDecorator : ComponentDecoratorBase<IMemberManager, IMemberProviderComponent>, IMemberProviderComponent
    {
        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly List<IMemberInfo> _members;

        [Preserve(Conditional = true)]
        public MethodMemberAccessorDecorator(IGlobalValueConverter? globalValueConverter = null, int priority = MemberComponentPriority.MethodAccessorDecorator)
            : base(priority)
        {
            _globalValueConverter = globalValueConverter;
            _members = new List<IMemberInfo>();
        }

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlag(MemberType.Accessor))
                return Components.TryGetMembers(memberManager, type, name, memberTypes, metadata);

            var methodArgsRaw = BindingMugenExtensions.GetMethodArgsRaw(name, out var methodName);
            if (methodName == null)
                return Components.TryGetMembers(memberManager, type, name, memberTypes, metadata);

            _members.Clear();
            Components.TryAddMembers(memberManager, _members, type, methodName, MemberType.Method, metadata);
            for (var i = 0; i < _members.Count; i++)
            {
                if (_members[i] is IMethodMemberInfo methodInfo)
                {
                    var parameters = methodInfo.GetParameters();
                    EnumFlags<ArgumentFlags> flags;
                    ItemOrArray<object?> values;
                    if (parameters.Count == 0)
                    {
                        flags = default;
                        values = default;
                    }
                    else
                        values = _globalValueConverter.TryGetInvokeArgs(parameters, methodArgsRaw, metadata, out flags);

                    if (parameters.Count == 0 || !values.IsEmpty)
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
    }
}