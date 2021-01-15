using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class MethodRequestMemberManagerDecorator : ComponentDecoratorBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent
    {
        private readonly List<IMemberInfo> _members;

        [Preserve(Conditional = true)]
        public MethodRequestMemberManagerDecorator(int priority = MemberComponentPriority.RequestHandlerDecorator)
            : base(priority)
        {
            _members = new List<IMemberInfo>();
        }

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, EnumFlags<MemberType> memberTypes, EnumFlags<MemberFlags> flags,
            object request,
            IReadOnlyMetadataContext? metadata)
        {
            if (request is not MemberTypesRequest typesRequest)
                return Components.TryGetMembers(memberManager, type, memberTypes, flags, request, metadata);

            _members.Clear();
            Owner.GetComponents<IMemberProviderComponent>(metadata).TryAddMembers(memberManager, _members, type, typesRequest.Name, memberTypes, metadata);
            if (_members.Count == 0)
                return default;

            var types = typesRequest.Types;
            var members = Components.TryGetMembers(memberManager, type, memberTypes, flags, _members, metadata);
            _members.Clear();
            foreach (var memberInfo in members)
            {
                if (memberInfo is not IMethodMemberInfo methodInfo)
                {
                    _members.Add(memberInfo);
                    continue;
                }

                var parameters = methodInfo.GetParameters();
                if (parameters.Count != types.Count)
                    continue;

                var isValid = true;
                for (var j = 0; j < types.Count; j++)
                {
                    if (parameters[j].ParameterType != types[j])
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                    _members.Add(methodInfo);
            }

            return _members.ToItemOrList(true);
        }
    }
}