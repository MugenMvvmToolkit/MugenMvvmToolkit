using System;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members
{
    public class DelegateObservableMemberInfo<TTarget, TState> : INotifiableMemberInfo where TTarget : class?
    {
        #region Fields

        private readonly RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? _raise;
        private readonly TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? _tryObserve;
        public readonly TState State;

        #endregion

        #region Constructors

        public DelegateObservableMemberInfo(string name, Type declaringType, Type memberType, MemberFlags accessModifiers, object? underlyingMember, in TState state,
            TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(declaringType, nameof(declaringType));
            Should.NotBeNull(memberType, nameof(memberType));
            Name = name;
            DeclaringType = declaringType;
            UnderlyingMember = underlyingMember;
            State = state;
            AccessModifiers = accessModifiers;
            Type = memberType;
            _tryObserve = tryObserve;
            _raise = raise;
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType { get; }

        public Type Type { get; }

        public object? UnderlyingMember { get; }

        public virtual MemberType MemberType => MemberType.Event;

        public MemberFlags AccessModifiers { get; }

        #endregion

        #region Implementation of interfaces

        public void Raise<T>(object? target, in T message, IReadOnlyMetadataContext? metadata = null)
        {
            _raise?.Invoke(this, (TTarget)target!, message, metadata);
        }

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_tryObserve == null)
                return default;
            return _tryObserve(this, (TTarget)target!, listener, metadata);
        }

        #endregion
    }
}