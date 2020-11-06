using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Components;
using MugenMvvm.Constants;
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
        private readonly IObservationManager? _observationManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MethodMemberAccessorDecorator(IGlobalValueConverter? globalValueConverter = null, IObservationManager? observationManager = null, int priority = ComponentPriority.Decorator)
            : base(priority)
        {
            _globalValueConverter = globalValueConverter;
            _observationManager = observationManager;
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes, IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlag(MemberType.Accessor))
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
                return ItemOrList.FromItem(memberInfo);
            }

            var result = _members.ToArray();
            _members.Clear();
            return ItemOrList.FromListToReadOnly(result);
        }

        #endregion
    }
}