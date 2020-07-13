using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class MethodRequestMemberManagerDecorator : ComponentDecoratorBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent, IHasPriority
    {
        #region Fields

        private readonly List<IMemberInfo> _members;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MethodRequestMemberManagerDecorator()
        {
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.RequestHandler;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(IMemberManager memberManager, Type type, MemberType memberTypes, MemberFlags flags, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TRequest) != typeof(MemberTypesRequest))
                return Components.TryGetMembers(memberManager, type, memberTypes, flags, request, metadata);

            var typesRequest = MugenExtensions.CastGeneric<TRequest, MemberTypesRequest>(request);
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