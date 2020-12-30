using System;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Observation;

namespace MugenMvvm.Bindings.Members.Builders
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct EventBuilder<TTarget> where TTarget : class?
    {
        #region Fields

        private readonly Type _declaringType;
        private readonly string _name;
        private readonly Type _eventType;
        private MemberAttachedDelegate<INotifiableMemberInfo, TTarget>? _attachedHandler;
        private TryObserveDelegate<INotifiableMemberInfo, TTarget>? _subscribe;
        private RaiseDelegate<INotifiableMemberInfo, TTarget>? _raise;
        private object? _underlyingMember;
        private bool _isStatic;

        #endregion

        #region Constructors

        public EventBuilder(string name, Type declaringType, Type eventType)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(declaringType, nameof(declaringType));
            Should.NotBeNull(eventType, nameof(eventType));
            _name = name;
            _declaringType = declaringType;
            _attachedHandler = null;
            _subscribe = null;
            _raise = null;
            _isStatic = false;
            _eventType = eventType;
            _underlyingMember = null;
        }

        #endregion

        #region Methods

        public EventBuilder<TTarget> Static()
        {
            _isStatic = true;
            return this;
        }

        public EventBuilder<TTarget> UnderlyingMember(object member)
        {
            Should.NotBeNull(member, nameof(member));
            _underlyingMember = member;
            return this;
        }

        public EventBuilder<TTarget> WrapMember(IObservableMemberInfo memberInfo)
        {
            Should.NotBeNull(memberInfo, nameof(memberInfo));
            return CustomImplementation(memberInfo.TryObserve, memberInfo is INotifiableMemberInfo notifiableMember ? notifiableMember.Raise : (RaiseDelegate<IObservableMemberInfo, TTarget>?) null);
        }

        public EventBuilder<TTarget> CustomImplementation(TryObserveDelegate<INotifiableMemberInfo, TTarget> subscribe, RaiseDelegate<IObservableMemberInfo, TTarget>? raise = null)
        {
            Should.NotBeNull(subscribe, nameof(subscribe));
            _subscribe = subscribe;
            _raise = raise;
            return this;
        }

        public EventBuilder<TTarget> AttachedHandler(MemberAttachedDelegate<INotifiableMemberInfo, TTarget> attachedHandler)
        {
            Should.NotBeNull(attachedHandler, nameof(attachedHandler));
            _attachedHandler = attachedHandler;
            return this;
        }

        public INotifiableMemberInfo Build()
        {
            if (_attachedHandler == null)
            {
                //custom implementation
                if (_subscribe != null)
                    return Event<object?>(null, _subscribe, _raise);

                //auto implementation
                var id = GenerateMemberId(true);
                return Event(id, (member, target, listener, metadata) => EventListenerCollection.GetOrAdd(member.GetTarget(target), member.State).Add(listener),
                    (member, target, message, metadata) => EventListenerCollection.Raise(member.GetTarget(target), member.State, message, metadata));
            }

            //auto implementation with attached handler
            var attachedHandlerId = GenerateMemberId(false);
            if (_subscribe == null)
            {
                var id = GenerateMemberId(true);
                return Event((id, attachedHandlerId, _attachedHandler), (member, target, listener, metadata) =>
                {
                    AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedHandlerId, target, member, member.State._attachedHandler, metadata);
                    return EventListenerCollection.GetOrAdd(member.GetTarget(target), member.State.id).Add(listener);
                }, (member, target, message, metadata) => EventListenerCollection.Raise(member.GetTarget(target), member.State.id, message, metadata));
            }

            //custom implementation with attached handler
            return Event((_subscribe, attachedHandlerId, _attachedHandler), (member, target, listener, metadata) =>
            {
                AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedHandlerId, target, member, member.State._attachedHandler, metadata);
                return member.State._subscribe(member, target, listener, metadata);
            }, _raise);
        }

        private string GenerateMemberId(bool isEventId) =>
            AttachedMemberBuilder.GenerateMemberId(isEventId ? BindingInternalConstant.AttachedEventPrefix : BindingInternalConstant.AttachedHandlerEventPrefix, _declaringType, _name);

        private DelegateObservableMemberInfo<TTarget, TState> Event<TState>(in TState state, TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget> tryObserve,
            RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise = null) =>
            new(_name, _declaringType, _eventType, AttachedMemberBuilder.GetFlags(_isStatic), _underlyingMember, state, false, tryObserve, raise);

        #endregion
    }
}