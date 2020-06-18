using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
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
        private readonly IObserverProvider? _observerProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MethodMemberAccessorDecorator(IGlobalValueConverter? globalValueConverter = null, IObserverProvider? observerProvider = null)
        {
            _globalValueConverter = globalValueConverter;
            _observerProvider = observerProvider;
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Decorator;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var methodArgsRaw = MugenBindingExtensions.GetMethodArgsRaw(name, out var methodName);
            if (methodArgsRaw == null)
                return Components.TryGetMembers(type, name, metadata);

            _members.Clear();
            Components.TryAddMembers(_members, type, methodName, metadata);
            for (var i = 0; i < _members.Count; i++)
            {
                if (_members[i] is IMethodMemberInfo methodInfo)
                {
                    var values = _globalValueConverter.TryGetInvokeArgs(methodInfo.GetParameters(), methodArgsRaw, metadata, out var flags);
                    if (values != null)
                    {
                        _members[i] = new MethodAccessorMemberInfo(methodName, methodInfo, null, values, flags, type, _observerProvider);
                        continue;
                    }
                }

                _members.RemoveAt(i);
                --i;
            }

            Components.TryAddMembers(_members, type, name, metadata);
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