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
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members
{
    public sealed class MethodMemberInfo : IMethodMemberInfo
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

        public MethodMemberInfo(string name, MethodInfo method, bool isExtensionMethodSupported, Type reflectedType, IObserverProvider? observerProvider, IReflectionDelegateProvider? reflectionDelegateProvider)
            : this(name, method, isExtensionMethodSupported, reflectedType, observerProvider, reflectionDelegateProvider, null, null)
        {
        }

        internal MethodMemberInfo(string name, MethodInfo method, bool isExtensionMethodSupported, Type reflectedType,
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
            {
                _genericArguments = genericArguments ?? _method.GetGenericArguments();
                for (var i = 0; i < _genericArguments.Length; i++)
                {
                    if (_genericArguments[i].IsGenericParameter)
                    {
                        IsGenericMethodDefinition = true;
                        break;
                    }
                }
            }

            if (parameterInfos == null)
                parameterInfos = _method.GetParameters();
            AccessModifiers = _method.GetAccessModifiers(isExtensionMethodSupported, ref parameterInfos);
            DeclaringType = AccessModifiers.HasFlagEx(MemberFlags.Extension) ? parameterInfos![0].ParameterType : method.DeclaringType;
            if (parameterInfos.Length == 0)
            {
                _parameters = Default.Array<IParameterInfo>();
                return;
            }

            var startIndex = AccessModifiers.HasFlagEx(MemberFlags.Extension) ? 1 : 0;
            var length = parameterInfos.Length - startIndex;
            if (length == 0)
                _parameters = Default.Array<IParameterInfo>();
            else
            {
                _parameters = new IParameterInfo[length];
                for (var i = 0; i < _parameters.Length; i++)
                    _parameters[i] = new ParameterInfoImpl(parameterInfos[i + startIndex]);
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

        public bool IsGenericMethodDefinition { get; }

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observerProvider.DefaultIfNull().GetMemberObserver(_reflectedType, this, metadata);
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

        public IMethodMemberInfo GetGenericMethodDefinition()
        {
            return new MethodMemberInfo(Name, _method.GetGenericMethodDefinition(), AccessModifiers.HasFlagEx(MemberFlags.Extension), _reflectedType, _observerProvider, _reflectionDelegateProvider);
        }

        public IMethodMemberInfo MakeGenericMethod(Type[] types)
        {
            var method = _method;
            if (IsGenericMethodDefinition)
                method = _method.GetGenericMethodDefinition();
            return new MethodMemberInfo(Name, method.MakeGenericMethod(types), AccessModifiers.HasFlagEx(MemberFlags.Extension), _reflectedType, _observerProvider, _reflectionDelegateProvider);
        }

        public object? Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null)
        {
            if (target != null && AccessModifiers.HasFlagEx(MemberFlags.Extension))
                return _invoker(null, args.InsertFirstArg(target));
            return _invoker(target, args);
        }

        #endregion

        #region Methods

        private object? CompileMethod(object? target, object?[] args)
        {
            _invoker = _method.GetMethodInvoker(_reflectionDelegateProvider);
            return Invoke(target, args);
        }

        #endregion
    }
}