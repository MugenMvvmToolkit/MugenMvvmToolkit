using System;
using System.Reflection;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members
{
    public sealed class MethodMemberInfo : IMethodMemberInfo
    {
        private readonly MethodInfo _method;
        private readonly ushort _modifiers;
        private readonly object? _parameters;
        private readonly Type _reflectedType;

        private Type[]? _genericArguments;
        private Func<object?, ItemOrArray<object?>, object?> _invoker;
        private MemberObserver _observer;

        public MethodMemberInfo(string name, MethodInfo method, bool isExtensionMethodSupported, Type reflectedType)
            : this(name, method, isExtensionMethodSupported, reflectedType, null, null)
        {
        }

        internal MethodMemberInfo(string name, MethodInfo method, bool isExtensionMethodSupported, Type reflectedType, ParameterInfo[]? parameterInfos, Type[]? genericArguments)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _method = method;
            _reflectedType = reflectedType;
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

            parameterInfos ??= _method.GetParameters();
            _modifiers = _method.GetAccessModifiers(isExtensionMethodSupported, ref parameterInfos).Value();
            DeclaringType = AccessModifiers.HasFlag(MemberFlags.Extension) ? parameterInfos![0].ParameterType : method.DeclaringType ?? typeof(object);
            if (parameterInfos.Length == 0)
                return;

            var startIndex = AccessModifiers.HasFlag(MemberFlags.Extension) ? 1 : 0;
            var length = parameterInfos.Length - startIndex;
            if (length == 0)
                return;

            if (length == 1)
                _parameters = new ParameterInfoImpl(parameterInfos[startIndex]);
            else
            {
                var parameters = new IParameterInfo[length];
                for (var i = 0; i < parameters.Length; i++)
                    parameters[i] = new ParameterInfoImpl(parameterInfos[i + startIndex]);
                _parameters = parameters;
            }
        }

        public string Name { get; }

        public Type DeclaringType { get; }

        public Type Type => _method.ReturnType;

        public object UnderlyingMember => _method;

        public MemberType MemberType => MemberType.Method;

        public EnumFlags<MemberFlags> AccessModifiers => new(_modifiers);

        public bool IsGenericMethod => _method.IsGenericMethod;

        public bool IsGenericMethodDefinition { get; }

        public ItemOrIReadOnlyList<IParameterInfo> GetParameters() => ItemOrIReadOnlyList.FromRawValue<IParameterInfo>(_parameters);

        public ItemOrIReadOnlyList<Type> GetGenericArguments() => _genericArguments ??= _method.GetGenericArguments();

        public IMethodMemberInfo GetGenericMethodDefinition()
            => new MethodMemberInfo(Name, _method.GetGenericMethodDefinition(), AccessModifiers.HasFlag(MemberFlags.Extension), _reflectedType);

        public IMethodMemberInfo MakeGenericMethod(ItemOrArray<Type> types)
        {
            var method = _method;
            if (IsGenericMethodDefinition)
                method = _method.GetGenericMethodDefinition();
            return new MethodMemberInfo(Name, method.MakeGenericMethod(types.AsList()), AccessModifiers.HasFlag(MemberFlags.Extension), _reflectedType);
        }

        public IAccessorMemberInfo? TryGetAccessor(EnumFlags<ArgumentFlags> argumentFlags, ItemOrIReadOnlyList<object?> args, IReadOnlyMetadataContext? metadata = null) => null;

        public object? Invoke(object? target, ItemOrArray<object?> args, IReadOnlyMetadataContext? metadata = null)
        {
            if (target != null && AccessModifiers.HasFlag(MemberFlags.Extension))
                return _invoker(null, args.InsertFirstArg(target));
            return _invoker(target, args);
        }

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer.IsEmpty)
                _observer = MugenService.ObservationManager.TryGetMemberObserver(_reflectedType, this, metadata).NoDoIfEmpty();
            return _observer.TryObserve(target, listener, metadata);
        }

        private object? CompileMethod(object? target, ItemOrArray<object?> args)
        {
            _invoker = _method.GetMethodInvoker();
            return Invoke(target, args);
        }
    }
}