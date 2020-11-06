using System;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Members
{
    public sealed class DelegateAccessorMemberInfo<TTarget, TValue, TState> : DelegateObservableMemberInfo<TTarget, TState>, IAccessorMemberInfo where TTarget : class?
    {
        #region Fields

        private readonly GetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? _getValue;
        private readonly SetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? _setValue;

        #endregion

        #region Constructors

        public DelegateAccessorMemberInfo(string name, Type declaringType, Type memberType, EnumFlags<MemberFlags> accessModifiers, object? underlyingMember, TState state,
            GetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? getValue, SetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? setValue,
            bool tryObserveByMember, TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise)
            : base(name, declaringType, memberType, accessModifiers, underlyingMember, state, tryObserveByMember, tryObserve, raise)
        {
            if (getValue == null)
                Should.NotBeNull(setValue, nameof(setValue));
            _getValue = getValue;
            _setValue = setValue;
        }

        #endregion

        #region Properties

        public bool CanRead => _getValue != null;

        public bool CanWrite => _setValue != null;

        public override MemberType MemberType => MemberType.Accessor;

        #endregion

        #region Implementation of interfaces

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata)
        {
            if (_getValue == null)
                BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
            return BoxingExtensions.Box(_getValue(this, (TTarget) target!, metadata));
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata)
        {
            if (_setValue == null)
                BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
            _setValue(this, (TTarget) target!, (TValue) value!, metadata);
        }

        #endregion
    }
}