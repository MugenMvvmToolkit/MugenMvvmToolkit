using System;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Members
{
    public sealed class DelegateMethodMemberInfo<TTarget, TReturnValue, TState> : DelegateObservableMemberInfo<TTarget, TState>, IMethodMemberInfo where TTarget : class?
    {
        private readonly Func<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>, ItemOrIReadOnlyList<IParameterInfo>>? _getParameters;
        private readonly InvokeMethodDelegate<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>, TTarget, TReturnValue> _invoke;
        private readonly TryGetAccessorDelegate<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>>? _tryGetAccessor;

        public DelegateMethodMemberInfo(string name, Type declaringType, Type memberType, EnumFlags<MemberFlags> accessModifiers, object? underlyingMember, TState state,
            InvokeMethodDelegate<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>, TTarget, TReturnValue> invoke,
            Func<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>, ItemOrIReadOnlyList<IParameterInfo>>? getParameters,
            TryGetAccessorDelegate<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>>? tryGetAccessor,
            TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise)
            : base(name, declaringType, memberType, accessModifiers, underlyingMember, state, tryObserve, raise)
        {
            Should.NotBeNull(invoke, nameof(invoke));
            _invoke = invoke;
            _getParameters = getParameters;
            _tryGetAccessor = tryGetAccessor;
        }

        public override MemberType MemberType => MemberType.Method;

        public bool IsGenericMethod => false;

        public bool IsGenericMethodDefinition => false;

        public override string ToString() => $"{Type} {Name}({string.Join(",", GetParameters().ToArray(info => $"{info.ParameterType} {info.Name}"))}) - {MemberFlags.ToString()}";

        public ItemOrIReadOnlyList<IParameterInfo> GetParameters()
        {
            if (_getParameters == null)
                return default;
            return _getParameters(this);
        }

        public ItemOrIReadOnlyList<Type> GetGenericArguments()
        {
            Should.MethodBeSupported(false, nameof(GetGenericArguments));
            return default;
        }

        public IMethodMemberInfo GetGenericMethodDefinition()
        {
            Should.MethodBeSupported(false, nameof(GetGenericMethodDefinition));
            return null!;
        }

        public IMethodMemberInfo MakeGenericMethod(ItemOrArray<Type> types)
        {
            Should.MethodBeSupported(false, nameof(MakeGenericMethod));
            return null!;
        }

        public IAccessorMemberInfo? TryGetAccessor(EnumFlags<ArgumentFlags> argumentFlags, ItemOrIReadOnlyList<object?> args, IReadOnlyMetadataContext? metadata = null) =>
            _tryGetAccessor?.Invoke(this, argumentFlags, args, metadata);

        public object? Invoke(object? target, ItemOrArray<object?> args, IReadOnlyMetadataContext? metadata = null) =>
            BoxingExtensions.Box(_invoke(this, (TTarget) target!, args, metadata));
    }
}