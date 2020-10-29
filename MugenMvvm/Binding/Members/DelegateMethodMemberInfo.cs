using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members
{
    public sealed class DelegateMethodMemberInfo<TTarget, TReturnValue, TState> : DelegateObservableMemberInfo<TTarget, TState>, IMethodMemberInfo where TTarget : class?
    {
        #region Fields

        private readonly Func<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>, IReadOnlyList<IParameterInfo>>? _getParameters;
        private readonly InvokeMethodDelegate<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>, TTarget, TReturnValue> _invoke;
        private readonly TryGetAccessorDelegate<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>>? _tryGetAccessor;

        #endregion

        #region Constructors

        public DelegateMethodMemberInfo(string name, Type declaringType, Type memberType, MemberFlags accessModifiers, object? underlyingMember, TState state,
            InvokeMethodDelegate<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>, TTarget, TReturnValue> invoke,
            Func<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>, IReadOnlyList<IParameterInfo>>? getParameters,
            TryGetAccessorDelegate<DelegateMethodMemberInfo<TTarget, TReturnValue, TState>>? tryGetAccessor, bool tryObserveByMember,
            TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise)
            : base(name, declaringType, memberType, accessModifiers, underlyingMember, state, tryObserveByMember, tryObserve, raise)
        {
            Should.NotBeNull(invoke, nameof(invoke));
            _invoke = invoke;
            _getParameters = getParameters;
            _tryGetAccessor = tryGetAccessor;
        }

        #endregion

        #region Properties

        public bool IsGenericMethod => false;

        public bool IsGenericMethodDefinition => false;

        public override MemberType MemberType => MemberType.Method;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IParameterInfo> GetParameters() => _getParameters?.Invoke(this) ?? Default.Array<IParameterInfo>();

        public IReadOnlyList<Type> GetGenericArguments()
        {
            Should.MethodBeSupported(false, nameof(GetGenericArguments));
            return null!;
        }

        public IMethodMemberInfo GetGenericMethodDefinition()
        {
            Should.MethodBeSupported(false, nameof(GetGenericMethodDefinition));
            return null!;
        }

        public IMethodMemberInfo MakeGenericMethod(Type[] types)
        {
            Should.MethodBeSupported(false, nameof(MakeGenericMethod));
            return null!;
        }

        public IAccessorMemberInfo? TryGetAccessor(ArgumentFlags argumentFlags, object?[]? args, IReadOnlyMetadataContext? metadata = null) => _tryGetAccessor?.Invoke(this, argumentFlags, args, metadata);

        public object? Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null) => BoxingExtensions.Box(_invoke(this, (TTarget) target!, args, metadata));

        #endregion
    }
}