using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class MethodMemberAccessorDecoratorComponent : DecoratorComponentBase<IMemberProvider, IMemberProviderComponent>, IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly List<IMemberInfo> _members;
        private readonly IObserverProvider? _observerProvider;

        #endregion

        #region Constructors

        public MethodMemberAccessorDecoratorComponent(IGlobalValueConverter? globalValueConverter, IObserverProvider? observerProvider)
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

        public IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var methodArgsRaw = MugenBindingExtensions.GetMethodArgsRaw(name, out var methodName);
            if (methodArgsRaw == null)
                return Components.TryGetMembers(type, methodName, metadata);

            _members.Clear();
            Components.TryAddMembers(_members, type, name, metadata);
            for (var i = 0; i < _members.Count; i++)
            {
                var methodInfo = _members[i] as IMethodInfo;
                if (methodInfo == null)
                    continue;

                var values = _globalValueConverter.TryGetInvokeArgs(methodInfo.GetParameters(), methodArgsRaw, metadata, out var isLastParameterMetadata);
                if (values != null)
                    _members.Add(new MethodMemberAccessorInfo(methodName, methodInfo, null, values, isLastParameterMetadata, type, _observerProvider));
            }

            return _members.ToArray();
        }

        #endregion
    }
}