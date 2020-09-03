using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Observation;

namespace MugenMvvm.Binding.Members.Builders
{
    public ref struct CustomPropertyBuilder<TTarget, TValue> where TTarget : class?
    {
        #region Fields

        private PropertyBuilder<TTarget, TValue> _propertyBuilder;
        private GetValueDelegate<IAccessorMemberInfo, TTarget, TValue>? _getter;
        private SetValueDelegate<IAccessorMemberInfo, TTarget, TValue>? _setter;
        private TryObserveDelegate<IObservableMemberInfo, TTarget>? _tryObserve;
        private RaiseDelegate<IObservableMemberInfo, TTarget>? _raise;
        private bool _isObservable;

        #endregion

        #region Constructors

        public CustomPropertyBuilder(PropertyBuilder<TTarget, TValue> propertyBuilder)
        {
            Should.BeSupported(!propertyBuilder.IsInherits, nameof(propertyBuilder.Inherits));
            Should.BeSupported(propertyBuilder.PropertyChanged == null, nameof(propertyBuilder.PropertyChangedHandler));
            Should.BeSupported(!propertyBuilder.HasDefaultValueField, nameof(propertyBuilder.DefaultValue));
            _propertyBuilder = propertyBuilder;
            _isObservable = false;
            _tryObserve = null;
            _raise = null;
            _getter = null;
            _setter = null;
        }

        #endregion

        #region Methods

        public CustomPropertyBuilder<TTarget, TValue> Static()
        {
            _propertyBuilder.Static();
            return this;
        }

        public CustomPropertyBuilder<TTarget, TValue> UnderlyingMember(object member)
        {
            _propertyBuilder.UnderlyingMember(member);
            return this;
        }

        public CustomPropertyBuilder<TTarget, TValue> CustomGetter(GetValueDelegate<IAccessorMemberInfo, TTarget, TValue> getter)
        {
            Should.NotBeNull(getter, nameof(getter));
            _getter = getter;
            return this;
        }

        public CustomPropertyBuilder<TTarget, TValue> CustomSetter(SetValueDelegate<IAccessorMemberInfo, TTarget, TValue> setter)
        {
            Should.NotBeNull(setter, nameof(setter));
            _setter = setter;
            return this;
        }

        public CustomPropertyBuilder<TTarget, TValue> Observable()
        {
            Should.BeSupported(_tryObserve == null, nameof(ObservableHandler));
            _isObservable = true;
            return this;
        }

        public CustomPropertyBuilder<TTarget, TValue> Observable(IObservableMemberInfo? memberInfo)
        {
            if (memberInfo == null)
                return this;
            return ObservableHandler(memberInfo.TryObserve, memberInfo is INotifiableMemberInfo notifiableMember ? notifiableMember.Raise : (RaiseDelegate<IObservableMemberInfo, TTarget>?) null);
        }

        public CustomPropertyBuilder<TTarget, TValue> ObservableHandler(TryObserveDelegate<IObservableMemberInfo, TTarget> tryObserve, RaiseDelegate<IObservableMemberInfo, TTarget>? raise = null)
        {
            Should.NotBeNull(tryObserve, nameof(tryObserve));
            Should.BeSupported(!_isObservable, nameof(Observable));
            _tryObserve = tryObserve;
            _raise = raise;
            return this;
        }

        public CustomPropertyBuilder<TTarget, TValue> NonObservable()
        {
            _isObservable = false;
            _propertyBuilder.NonObservable();
            return this;
        }

        public CustomPropertyBuilder<TTarget, TValue> AttachedHandler(MemberAttachedDelegate<IAccessorMemberInfo, TTarget> attachedHandler)
        {
            _propertyBuilder.AttachedHandler(attachedHandler);
            return this;
        }

        public IAccessorMemberInfo Build()
        {
            _propertyBuilder.WrapperClosure?.SetFlags(_propertyBuilder.IsStatic);
            var id = _isObservable ? _propertyBuilder.GenerateMemberId(true) : null;
            if (_propertyBuilder.AttachedHandlerField == null)
            {
                if (!_isObservable)
                    return _propertyBuilder.Property<object?>(null, _getter, _setter, _tryObserve, _raise);

                return _propertyBuilder.Property(id!, _getter, _setter, (member, target, listener, metadata) => EventListenerCollection.GetOrAdd(member.GetTarget(target), member.State).Add(listener),
                    (member, target, message, metadata) => EventListenerCollection.Raise(member.GetTarget(target), member.State, message, metadata));
            }

            RaiseDelegate<DelegateObservableMemberInfo<TTarget, (GetValueDelegate<IAccessorMemberInfo, TTarget, TValue>? _getter, SetValueDelegate<IAccessorMemberInfo, TTarget, TValue>? _setter,
                TryObserveDelegate<IObservableMemberInfo, TTarget>? _tryObserve, MemberAttachedDelegate<IAccessorMemberInfo, TTarget> AttachedHandlerField, string attachedId, string? id)>, TTarget>? raise = null;
            if (id != null)
                raise = (member, target, message, metadata) => EventListenerCollection.Raise(member.GetTarget(target), member.State.id!, message, metadata);
            var attachedId = _propertyBuilder.GenerateMemberId(false);
            return Property((_getter, _setter, _tryObserve, _propertyBuilder.AttachedHandlerField, attachedId, id), (member, target, metadata) =>
            {
                AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedId, target, member, member.State.AttachedHandlerField!, metadata);
                return member.State._getter!(member, target, metadata);
            }, (member, target, value, metadata) =>
            {
                AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedId, target, member, member.State.AttachedHandlerField!, metadata);
                member.State._setter!(member, target, value, metadata);
            }, (member, target, listener, metadata) =>
            {
                AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedId, target, (IAccessorMemberInfo) member, member.State.AttachedHandlerField!, metadata);
                if (member.State.id == null)
                    return member.State._tryObserve!(member, target, listener, metadata);
                return EventListenerCollection.GetOrAdd(member.GetTarget(target), member.State.id).Add(listener);
            }, raise ?? _raise);
        }

        private DelegateAccessorMemberInfo<TTarget, TValue, TState> Property<TState>(in TState state,
            GetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? getValue, SetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? setValue,
            TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise) =>
            _propertyBuilder.Property(state, _getter == null ? null : getValue, _setter == null ? null : setValue,
                _tryObserve == null && !_isObservable ? null : tryObserve, raise);

        #endregion
    }
}