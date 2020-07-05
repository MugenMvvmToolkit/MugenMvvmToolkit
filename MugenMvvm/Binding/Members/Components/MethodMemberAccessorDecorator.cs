using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class MethodMemberAccessorDecorator : ComponentDecoratorBase<IMemberManager, IMemberProviderComponent>, IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly List<IMemberInfo> _members;
        private readonly IObservationManager? _observationManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MethodMemberAccessorDecorator(IGlobalValueConverter? globalValueConverter = null, IObservationManager? observationManager = null)
        {
            _globalValueConverter = globalValueConverter;
            _observationManager = observationManager;
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Decorator;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlagEx(MemberType.Accessor))
                return Components.TryGetMembers(memberManager, type, name, memberTypes, metadata);

            var methodArgsRaw = MugenBindingExtensions.GetMethodArgsRaw(name, out var methodName);
            if (methodArgsRaw == null)
                return Components.TryGetMembers(memberManager, type, name, memberTypes, metadata);

            _members.Clear();
            Components.TryAddMembers(memberManager, _members, type, methodName, MemberType.Method, metadata);
            for (var i = 0; i < _members.Count; i++)
            {
                if (_members[i] is IMethodMemberInfo methodInfo)
                {
                    var values = _globalValueConverter.TryGetInvokeArgs(methodInfo.GetParameters(), methodArgsRaw, metadata, out var flags);
                    if (values != null)
                    {
                        _members[i] = methodInfo.TryGetAccessor(flags, values, metadata) ?? new MethodAccessorMemberInfo(methodName, methodInfo, null, values, flags, type, _observationManager);
                        continue;
                    }
                }

                _members.RemoveAt(i);
                --i;
            }

            Components.TryAddMembers(memberManager, _members, type, name, memberTypes, metadata);
            if (_members.Count == 1)
            {
                var memberInfo = _members[0];
                _members.Clear();
                return new ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>(memberInfo);
            }

            var result = _members.ToArray();
            _members.Clear();
            return result;
        }

        #endregion
    }
}