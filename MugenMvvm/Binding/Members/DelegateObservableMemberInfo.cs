using System;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members
{
    public class DelegateObservableMemberInfo<TTarget, TState> : INotifiableMemberInfo where TTarget : class?
    {
        #region Fields

        private readonly RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? _raise;
        private readonly TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? _tryObserve;
        public readonly TState State;
        private MemberObserver _observer;

        #endregion

        #region Constructors

        public DelegateObservableMemberInfo(string name, Type declaringType, Type memberType, MemberFlags accessModifiers, object? underlyingMember, TState state, bool tryObserveByMember,
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
            if (!tryObserveByMember)
                _observer = MemberObserver.NoDo;
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

        public void Raise(object? target, object? message = null, IReadOnlyMetadataContext? metadata = null) => _raise?.Invoke(this, (TTarget) target!, message, metadata);

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_tryObserve == null)
            {
                if (_observer.IsEmpty)
                    _observer = MugenBindingService.ObservationManager.TryGetMemberObserver(DeclaringType, this, metadata).NoDoIfEmpty();

                return _observer.TryObserve(target, listener, metadata);
            }

            return _tryObserve(this, (TTarget) target!, listener, metadata);
        }

        #endregion
    }
}