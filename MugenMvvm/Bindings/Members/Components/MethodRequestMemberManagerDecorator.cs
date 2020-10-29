using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class MethodRequestMemberManagerDecorator : ComponentDecoratorBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent
    {
        #region Fields

        private readonly List<IMemberInfo> _members;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MethodRequestMemberManagerDecorator(int priority = MemberComponentPriority.RequestHandler)
            : base(priority)
        {
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, MemberType memberTypes, MemberFlags flags, object request, IReadOnlyMetadataContext? metadata)
        {
            if (!(request is MemberTypesRequest typesRequest))
                return Components.TryGetMembers(memberManager, type, memberTypes, flags, request, metadata);

            _members.Clear();
            Owner.GetComponents<IMemberProviderComponent>(metadata).TryAddMembers(memberManager, _members, type, typesRequest.Name, memberTypes, metadata);
            if (_members.Count == 0)
                return default;

            var types = typesRequest.Types;
            var iterator = Components.TryGetMembers(memberManager, type, memberTypes, flags, _members, metadata).Iterator();
            _members.Clear();
            foreach (var memberInfo in iterator)
            {
                if (!(memberInfo is IMethodMemberInfo methodInfo))
                {
                    _members.Add(memberInfo);
                    continue;
                }

                var parameters = methodInfo.GetParameters();
                if (parameters.Count != types.Length)
                    continue;

                var isValid = true;
                for (var j = 0; j < types.Length; j++)
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

            if (_members.Count == 0)
                return default;
            if (_members.Count == 1)
                return ItemOrList.FromItem(_members[0]);
            return ItemOrList.FromListToReadOnly(_members.ToArray());
        }

        #endregion
    }
}