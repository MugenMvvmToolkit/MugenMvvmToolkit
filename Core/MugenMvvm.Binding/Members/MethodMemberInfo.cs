using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class MethodMemberInfo : IMethodInfo
    {
        #region Fields

        private readonly Type[]? _genericArguments;

        private readonly MethodInfo _method;
        private readonly IObserverProvider? _observerProvider;
        private readonly IParameterInfo[] _parameters;
        private readonly Type _reflectedType;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private Func<object?, object?[], object?> _invoker;

        private MemberObserver? _observer;

        #endregion

        #region Constructors

        public MethodMemberInfo(string name, MethodInfo method, bool checkExtensionMethod, Type reflectedType, IObserverProvider? observerProvider,
            IReflectionDelegateProvider? reflectionDelegateProvider)
            : this(name, method, checkExtensionMethod, reflectedType, observerProvider, reflectionDelegateProvider, null, null)
        {
        }

        internal MethodMemberInfo(string name, MethodInfo method, bool checkExtensionMethod, Type reflectedType,
            IObserverProvider? observerProvider, IReflectionDelegateProvider? reflectionDelegateProvider, ParameterInfo[]? parameterInfos, Type[]? genericArguments)
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
            if (_method.IsGenericMethod || _method.IsGenericMethodDefinition)
                _genericArguments = genericArguments ?? _method.GetGenericArguments();

            if (parameterInfos == null)
                parameterInfos = _method.GetParameters();
            AccessModifiers = _method.GetAccessModifiers(checkExtensionMethod, ref parameterInfos);
            DeclaringType = AccessModifiers.HasFlagEx(MemberFlags.Extension) ? parameterInfos![0].ParameterType : method.DeclaringType;
            if (parameterInfos.Length == 0)
            {
                _parameters = Default.EmptyArray<IParameterInfo>();
                return;
            }

            var startIndex = AccessModifiers.HasFlagEx(MemberFlags.Extension) ? 1 : 0;
            var l = parameterInfos.Length - startIndex;
            if (l == 0)
                _parameters = Default.EmptyArray<IParameterInfo>();
            else
            {
                _parameters = new IParameterInfo[l];
                for (var i = 0; i < _parameters.Length; i++)
                    _parameters[i] = new Parameter(parameterInfos[i + startIndex]);
            }
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType { get; }

        public Type Type => _method.ReturnType;

        public object? UnderlyingMember => _method;

        public MemberType MemberType => MemberType.Method;

        public MemberFlags AccessModifiers { get; }

        public bool IsGenericMethod => _method.IsGenericMethod;

        public bool IsGenericMethodDefinition => IsNotResolvedGeneric();

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observerProvider.DefaultIfNull().TryGetMemberObserver(_reflectedType, _method);
            return _observer.Value.TryObserve(target, listener, metadata);
        }

        public IReadOnlyList<IParameterInfo> GetParameters()
        {
            return _parameters;
        }

        public IReadOnlyList<Type> GetGenericArguments()
        {
            return _genericArguments ?? _method.GetGenericArguments();
        }

        public IMethodInfo MakeGenericMethod(Type[] types)
        {
            var method = _method;
            if (!method.IsGenericMethodDefinition && IsNotResolvedGeneric())
                method = _method.GetGenericMethodDefinition();
            return new MethodMemberInfo(Name, method.MakeGenericMethod(types), AccessModifiers.HasFlagEx(MemberFlags.Extension), _reflectedType, _observerProvider,
                _reflectionDelegateProvider);
        }

        public object? Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null)
        {
            if (target != null && AccessModifiers.HasFlagEx(MemberFlags.Extension))
                return _invoker(null, args.InsertFirstArg(target));
            return _invoker(target, args);
        }

        #endregion

        #region Methods

        private bool IsNotResolvedGeneric()
        {
            if (_genericArguments == null)
                return false;
            for (var i = 0; i < _genericArguments.Length; i++)
            {
                if (_genericArguments[i].IsGenericParameter)
                    return true;
            }

            return false;
        }

        private object? CompileMethod(object? target, object?[] args)
        {
            _invoker = _method.GetMethodInvoker(_reflectionDelegateProvider);
            return _invoker(target, args);
        }

        #endregion

        #region Nested types

        private sealed class Parameter : IParameterInfo
        {
            #region Fields

            private readonly ParameterInfo _parameterInfo;

            #endregion

            #region Constructors

            public Parameter(ParameterInfo parameterInfo)
            {
                _parameterInfo = parameterInfo;
            }

            #endregion

            #region Properties

            public object? UnderlyingParameter => _parameterInfo;

            public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

            public Type ParameterType => _parameterInfo.ParameterType;

            public object? DefaultValue => _parameterInfo.DefaultValue;

            #endregion

            #region Implementation of interfaces

            public bool IsDefined(Type type)
            {
                return _parameterInfo.IsDefined(typeof(ParamArrayAttribute), false);
            }

            #endregion
        }

        #endregion
    }
}