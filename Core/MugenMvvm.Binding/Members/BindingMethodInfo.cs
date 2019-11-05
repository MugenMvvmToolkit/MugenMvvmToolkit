﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class BindingMethodInfo : IBindingMethodInfo
    {
        #region Fields

        private readonly MethodInfo _method;
        private readonly IObserverProvider? _observerProvider;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly IBindingParameterInfo[] _parameters;
        private readonly Type _reflectedType;
        private Func<object?, object?[], object?> _invoker;

        private MemberObserver? _observer;

        #endregion

        #region Constructors

        public BindingMethodInfo(string name, MethodInfo method, Type reflectedType, IObserverProvider? observerProvider, IReflectionDelegateProvider? reflectionDelegateProvider)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _method = method;
            _reflectedType = reflectedType;
            _observerProvider = observerProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _invoker = CompileMethod;
            Name = name;
            AccessModifiers = _method.GetAccessModifiers();
            var parameterInfos = method.GetParameters();
            if (parameterInfos.Length == 0)
                _parameters = Default.EmptyArray<IBindingParameterInfo>();
            else
            {
                _parameters = new IBindingParameterInfo[parameterInfos.Length];
                for (var i = 0; i < parameterInfos.Length; i++)
                    _parameters[i] = new Parameter(parameterInfos[i]);
            }

            IsExtensionMethod = method.IsStatic && method.IsDefined(typeof(ExtensionAttribute), false) && _parameters.Length > 0;
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type Type => _method.ReturnType;

        public object? Member => _method;

        public BindingMemberType MemberType => BindingMemberType.Method;

        public BindingMemberFlags AccessModifiers { get; }

        public bool IsExtensionMethod { get; }

        public bool IsGenericMethod => _method.IsGenericMethod;

        public bool IsGenericMethodDefinition => _method.IsGenericMethodDefinition;

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observerProvider.ServiceIfNull().TryGetMemberObserver(_reflectedType, _method);
            return _observer.Value.TryObserve(target, listener, metadata);
        }

        public IBindingParameterInfo[] GetParameters()
        {
            return _parameters;
        }

        public Type[] GetGenericArguments()
        {
            return _method.GetGenericArguments();
        }

        public IBindingMethodInfo MakeGenericMethod(Type[] types)
        {
            return new BindingMethodInfo(Name, _method.MakeGenericMethod(types), _reflectedType, _observerProvider, _reflectionDelegateProvider);
        }

        public object? Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null)
        {
            return _invoker(target, args);
        }

        #endregion

        #region Methods

        private object? CompileMethod(object? target, object?[] args)
        {
            _invoker = _method.GetMethodInvoker(_reflectionDelegateProvider);
            return _invoker(target, args);
        }

        #endregion

        #region Nested types

        private sealed class Parameter : IBindingParameterInfo
        {
            #region Fields

            private readonly ParameterInfo _parameterInfo;

            #endregion

            #region Constructors

            public Parameter(ParameterInfo parameterInfo)
            {
                _parameterInfo = parameterInfo;
                IsParamsArray = parameterInfo.IsDefined(typeof(ParamArrayAttribute), true);
            }

            #endregion

            #region Properties

            public bool IsParamsArray { get; }

            public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

            public Type ParameterType => _parameterInfo.ParameterType;

            public object? DefaultValue => _parameterInfo.DefaultValue;

            #endregion
        }

        #endregion
    }
}