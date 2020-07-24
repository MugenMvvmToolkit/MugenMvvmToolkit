using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Builders
{
    public ref struct PropertyBuilder<TTarget, TValue> where TTarget : class?
    {
        #region Fields

        internal readonly Type DeclaringType;
        internal readonly string Name;
        internal ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>? PropertyChanged;
        internal MemberAttachedDelegate<IAccessorMemberInfo, TTarget>? AttachedHandlerField;
        internal Func<IAccessorMemberInfo, TTarget, TValue>? GetDefaultValue;
        internal TValue DefaultValueField;
        internal object? UnderlyingMemberField;
        internal Type PropertyType;
        internal bool IsStatic;
        internal bool IsInherits;
        internal MemberWrapperClosure? WrapperClosure;
        internal bool HasDefaultValueField;
        internal bool IsNonObservable;

        #endregion

        #region Constructors

        public PropertyBuilder(string name, Type declaringType, Type propertyType)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(declaringType, nameof(declaringType));
            Should.NotBeNull(propertyType, nameof(propertyType));
            Name = name;
            DeclaringType = declaringType;
            AttachedHandlerField = null;
            PropertyChanged = null;
            GetDefaultValue = null;
            IsStatic = false;
            IsInherits = false;
            WrapperClosure = null;
            IsNonObservable = false;
            DefaultValueField = default!;
            HasDefaultValueField = false;
            PropertyType = propertyType;
            UnderlyingMemberField = null;
        }

        #endregion

        #region Methods

        public PropertyBuilder<TTarget, TValue> Static()
        {
            Should.BeSupported(!IsInherits, nameof(Inherits));
            IsStatic = true;
            return this;
        }

        public PropertyBuilder<TTarget, TValue> Inherits()
        {
            Should.BeSupported(!IsStatic, nameof(Static));
            IsInherits = true;
            return this;
        }

        public PropertyBuilder<TTarget, TValue> DefaultValue(TValue defaultValue)
        {
            HasDefaultValueField = true;
            DefaultValueField = defaultValue;
            return this;
        }

        public PropertyBuilder<TTarget, TValue> DefaultValue(Func<IAccessorMemberInfo, TTarget, TValue> getDefaultValue)
        {
            Should.NotBeNull(getDefaultValue, nameof(getDefaultValue));
            HasDefaultValueField = true;
            GetDefaultValue = getDefaultValue;
            return this;
        }

        public PropertyBuilder<TTarget, TValue> UnderlyingMember(object member)
        {
            Should.NotBeNull(member, nameof(member));
            UnderlyingMemberField = member;
            return this;
        }

        public CustomPropertyBuilder<TTarget, TValue> CustomGetter(GetValueDelegate<IAccessorMemberInfo, TTarget, TValue> getter)
        {
            return new CustomPropertyBuilder<TTarget, TValue>(this).CustomGetter(getter);
        }

        public CustomPropertyBuilder<TTarget, TValue> CustomSetter(SetValueDelegate<IAccessorMemberInfo, TTarget, TValue> setter)
        {
            return new CustomPropertyBuilder<TTarget, TValue>(this).CustomSetter(setter);
        }

        public CustomPropertyBuilder<TTarget, TValue> WrapMember(string memberName, MemberFlags memberFlags = MemberFlags.All)
        {
            WrapperClosure = new MemberWrapperClosure(memberName, memberFlags);
            return CustomGetter(WrapperClosure.GetValue).CustomSetter(WrapperClosure.SetValue).ObservableHandler(WrapperClosure.TryObserve);
        }

        public PropertyBuilder<TTarget, TValue> NonObservable()
        {
            IsNonObservable = true;
            return this;
        }

        public PropertyBuilder<TTarget, TValue> AttachedHandler(MemberAttachedDelegate<IAccessorMemberInfo, TTarget> attachedHandler)
        {
            Should.NotBeNull(attachedHandler, nameof(attachedHandler));
            AttachedHandlerField = attachedHandler;
            return this;
        }

        public PropertyBuilder<TTarget, TValue> PropertyChangedHandler(ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue> propertyChanged)
        {
            Should.NotBeNull(propertyChanged, nameof(propertyChanged));
            PropertyChanged = propertyChanged;
            return this;
        }

        public IAccessorMemberInfo Build()
        {
            WrapperClosure?.SetFlags(IsStatic);
            var id = GenerateMemberId(true);
            if (IsInherits)
            {
                return Property((id, AttachedHandlerField, PropertyChanged, DefaultValueField, GetDefaultValue),
                    (member, target, metadata) => InheritedProperty.GetOrAdd(target, member, metadata).Value,
                    (member, target, value, metadata) => InheritedProperty.GetOrAdd(target, member, metadata).SetValue(target!, value, metadata),
                    (member, target, listener, metadata) => InheritedProperty.GetOrAdd(target, member, metadata).Add(listener), null);
            }

            return Property((id, AttachedHandlerField, PropertyChanged, DefaultValueField, GetDefaultValue),
                (member, target, metadata) => AutoProperty.GetOrAdd(target, member, metadata).Value,
                (member, target, value, metadata) => AutoProperty.GetOrAdd(target, member, metadata).SetValue(member, target, value, metadata),
                (member, target, listener, metadata) => AutoProperty.GetOrAdd(target, member, metadata).Add(listener), null);
        }

        internal string GenerateMemberId(bool isPropertyId)
        {
            return AttachedMemberBuilder.GenerateMemberId(isPropertyId ? BindingInternalConstant.AttachedPropertyPrefix : BindingInternalConstant.AttachedHandlerPropertyPrefix, DeclaringType, Name);
        }

        internal DelegateAccessorMemberInfo<TTarget, TValue, TState> Property<TState>(in TState state,
            GetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? getValue, SetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? setValue,
            TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise)
        {
            return new DelegateAccessorMemberInfo<TTarget, TValue, TState>(Name, DeclaringType, PropertyType,
                AttachedMemberBuilder.GetFlags(IsStatic), UnderlyingMemberField, state, getValue, setValue, IsNonObservable ? null : tryObserve, raise);
        }

        #endregion

        #region Nested types

        internal sealed class MemberWrapperClosure
        {
            #region Fields

            private MemberFlags _flags;
            private readonly string _key;
            private readonly string _memberName;

            #endregion

            #region Constructors

            public MemberWrapperClosure(string memberName, MemberFlags flags)
            {
                Should.NotBeNull(memberName, nameof(memberName));
                _memberName = memberName;
                _key = BindingInternalConstant.WrapMemberPrefix + memberName;
                _flags = flags;
            }

            #endregion

            #region Methods

            public void SetFlags(bool isStatic)
            {
                _flags = _flags.SetInstanceOrStaticFlags(isStatic);
            }

            public TValue GetValue(IAccessorMemberInfo member, TTarget target, IReadOnlyMetadataContext? metadata)
            {
                return MemberWrapper.GetOrAdd(member.GetTarget(target), _key, _memberName, _flags, metadata).GetValue(target, metadata);
            }

            public void SetValue(IAccessorMemberInfo member, TTarget target, TValue value, IReadOnlyMetadataContext? metadata)
            {
                MemberWrapper.GetOrAdd(member.GetTarget(target), _key, _memberName, _flags, metadata).SetValue(target, value, metadata);
            }

            public ActionToken TryObserve(IObservableMemberInfo member, TTarget target, IEventListener listener, IReadOnlyMetadataContext? metadata)
            {
                return MemberWrapper.GetOrAdd(member.GetTarget(target), _key, _memberName, _flags, metadata).TryObserve(target, listener, metadata);
            }

            #endregion
        }

        private sealed class MemberWrapper : EventListenerCollection, IWeakEventListener
        {
            #region Fields

            private ActionToken _listener;
            private IAccessorMemberInfo? _member;
            private TValue _value;

            #endregion

            #region Constructors

            private MemberWrapper(IAccessorMemberInfo? member)
            {
                _member = member;
                _value = default!;
            }

            #endregion

            #region Properties

            public bool IsAlive => true;

            public bool IsWeak => true;

            #endregion

            #region Implementation of interfaces

            bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
            {
                try
                {
                    Raise(sender, message, metadata);
                }
                catch (Exception e)
                {
                    MugenService.Application.OnUnhandledException(e, UnhandledExceptionType.Binding, metadata);
                }
                return true;
            }

            #endregion

            #region Methods

            public static MemberWrapper GetOrAdd(object target, string key, string wrapMemberName, MemberFlags flags, IReadOnlyMetadataContext? metadata)
            {
                Should.NotBeNull(target, nameof(target));
                var attachedValues = MugenService.AttachedValueManager.TryGetAttachedValues(target, metadata);
                if (attachedValues.TryGet(key, out var value))
                    return (MemberWrapper)value!;

                return attachedValues.GetOrAdd(key, (wrapMemberName, flags, metadata), (o, s) =>
                {
                    var member = MugenBindingService
                        .MemberManager
                        .TryGetMember(s.flags.GetTargetType(ref o!), MemberType.Accessor, s.flags, s.wrapMemberName, s.metadata) as IAccessorMemberInfo;
                    return new MemberWrapper(member);
                });
            }

            public TValue GetValue(TTarget target, IReadOnlyMetadataContext? metadata)
            {
                var member = _member;
                if (member == null)
                    return _value;
                return (TValue)member.GetValue(target, metadata)!;
            }

            public void SetValue(TTarget target, TValue value, IReadOnlyMetadataContext? metadata)
            {
                _value = value;
                if (_member != null)
                {
                    _listener.Dispose();
                    _member = null;
                }

                Raise(target, metadata, metadata);
            }

            public ActionToken TryObserve(TTarget target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
            {
                if (Count == 0)
                {
                    var member = _member;
                    if (member != null)
                        _listener = member.TryObserve(target, this, metadata);
                }

                return Add(listener);
            }

            protected override void OnListenersRemoved()
            {
                _listener.Dispose();
            }

            #endregion
        }

        private sealed class InheritedProperty : EventListenerCollection, IWeakEventListener
        {
            #region Fields

            private readonly DelegateObservableMemberInfo<TTarget, (string id, MemberAttachedDelegate<IAccessorMemberInfo, TTarget>? AttachedHandlerField, ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>?
                PropertyChanged, TValue DefaultValueField, Func<IAccessorMemberInfo, TTarget, TValue>? GetDefaultValue)> _member;

            private readonly IWeakReference _targetRef;
            private IWeakReference? _parentRef;
            private ActionToken _parentToken;
            private byte _state;
            public TValue Value;

            private const byte DefaultState = 0;
            private const byte ParentState = 1;
            private const byte HasValueState = 2;

            #endregion

            #region Constructors

            private InheritedProperty(TTarget target,
                DelegateObservableMemberInfo<TTarget, (string id, MemberAttachedDelegate<IAccessorMemberInfo, TTarget>? AttachedHandlerField, ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>? PropertyChanged,
                    TValue DefaultValueField, Func<IAccessorMemberInfo, TTarget, TValue>? GetDefaultValue)> member, IReadOnlyMetadataContext? metadata)
            {
                _targetRef = target.ToWeakReference();
                _member = member;
                Value = member.State.GetDefaultValue == null ? member.State.DefaultValueField : member.State.GetDefaultValue((IAccessorMemberInfo)member, target);
                InvalidateParent(target!, metadata);
            }

            #endregion

            #region Properties

            public bool IsAlive => _targetRef.IsAlive;

            public bool IsWeak => true;

            #endregion

            #region Implementation of interfaces

            bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
            {
                try
                {
                    var target = (TTarget)_targetRef.Target;
                    if (target == null)
                        return false;
                    if (message is InheritedProperty inheritedProperty)
                        ApplyValues(target, inheritedProperty, metadata);
                    else
                        InvalidateParent(target, metadata);
                }
                catch (Exception e)
                {
                    MugenService.Application.OnUnhandledException(e, UnhandledExceptionType.Binding, metadata);
                }
                return true;
            }

            #endregion

            #region Methods

            public static InheritedProperty GetOrAdd(TTarget target,
                DelegateObservableMemberInfo<TTarget, (string id, MemberAttachedDelegate<IAccessorMemberInfo, TTarget>? AttachedHandlerField, ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>? PropertyChanged,
                    TValue DefaultValueField, Func<IAccessorMemberInfo, TTarget, TValue>? GetDefaultValue)> member, IReadOnlyMetadataContext? metadata)
            {
#pragma warning disable 8634
                var attachedValues = MugenService.AttachedValueManager.TryGetAttachedValues(target!, metadata);
                if (attachedValues.TryGet(member.State.id, out var value))
                    return (InheritedProperty)value!;
                return attachedValues.GetOrAdd(member.State.id, (member, metadata), (t, state) =>
                {
                    state.member.State.AttachedHandlerField?.Invoke((IAccessorMemberInfo)state.member, (TTarget)t, state.metadata);
                    return new InheritedProperty((TTarget)t, state.member, state.metadata);
                });
#pragma warning restore 8634
            }

            public void SetValue(TTarget target, TValue value, IReadOnlyMetadataContext? metadata)
            {
                SetValue(target, value, HasValueState, metadata);
            }

            private void SetValue(TTarget target, TValue value, byte state, IReadOnlyMetadataContext? metadata)
            {
                if (state == HasValueState)
                {
                    if (!_parentToken.IsEmpty)
                        _parentToken.Dispose();
                    if (_parentRef != null)
                    {
                        TryUnsubscribe((TTarget)_parentRef?.Target!, metadata);
                        _parentRef = null;
                    }
                }
                else if (_state == HasValueState)
                    return;

                _state = state;
                var oldValue = Value;
                if (EqualityComparer<TValue>.Default.Equals(oldValue, value))
                    return;
                Value = value;
                _member.State.PropertyChanged?.Invoke((IAccessorMemberInfo)_member, target, oldValue, value, metadata);
                Raise(target, this, metadata);
            }

            private void InvalidateParent([DisallowNull] TTarget target, IReadOnlyMetadataContext? metadata)
            {
                var member = BindableMembers.For<object>().Parent().TryGetMember(target.GetType(), MemberFlags.InstancePublicAll, metadata);
                var oldParent = _parentRef?.Target;
                var parent = member?.GetValue(target, metadata) as TTarget;
                if (ReferenceEquals(oldParent, parent))
                {
                    if (member != null && _parentRef == null && _parentToken.IsEmpty)
                        _parentToken = member.TryObserve(target, this, metadata);
                    return;
                }

                TryUnsubscribe((TTarget)oldParent!, metadata);
                if (member != null && _parentToken.IsEmpty)
                    _parentToken = member.TryObserve(target, this, metadata);
                if (parent == null)
                {
                    _parentRef = null;
                    ApplyValues(target, null, metadata);
                    return;
                }

                _parentRef = parent.ToWeakReference();
                var inheritedProperty = GetOrAdd(parent, _member, metadata);
                ApplyValues(target, inheritedProperty, metadata);
                inheritedProperty.Add(this);
            }

            private void ApplyValues([DisallowNull] TTarget target, InheritedProperty? parentProperty, IReadOnlyMetadataContext? metadata)
            {
                if (parentProperty != null && parentProperty._state != DefaultState)
                    SetValue(target, parentProperty.Value, ParentState, metadata);
                else if (_state == ParentState)
                    SetValue(target, _member.State.GetDefaultValue == null ? _member.State.DefaultValueField : _member.State.GetDefaultValue((IAccessorMemberInfo)_member, target), DefaultState, metadata);
            }

            private void TryUnsubscribe(TTarget parent, IReadOnlyMetadataContext? metadata)
            {
                if (parent != null)
                    GetOrAdd(parent, _member, metadata).Remove(this);
            }

            #endregion
        }

        private sealed class AutoProperty : EventListenerCollection
        {
            #region Fields

            private readonly ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>? _propertyChanged;
            public TValue Value;

            #endregion

            #region Constructors

            private AutoProperty(ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>? propertyChanged, TValue defaultValue)
            {
                _propertyChanged = propertyChanged;
                Value = defaultValue;
            }

            #endregion

            #region Methods

            public static AutoProperty GetOrAdd(TTarget target,
                DelegateObservableMemberInfo<TTarget, (string id, MemberAttachedDelegate<IAccessorMemberInfo, TTarget>? AttachedHandlerField, ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>? PropertyChanged,
                    TValue DefaultValueField, Func<IAccessorMemberInfo, TTarget, TValue>? GetDefaultValue)> member, IReadOnlyMetadataContext? metadata)
            {
                var attachedValues = MugenService.AttachedValueManager.TryGetAttachedValues(member.GetTarget(target), metadata);
                if (attachedValues.TryGet(member.State.id, out var value))
                    return (AutoProperty)value!;
                return attachedValues.GetOrAdd(member.State.id, (member, metadata), (t, s) =>
                {
                    if (s.member.AccessModifiers.HasFlagEx(MemberFlags.Static))
                        t = null!;
                    s.member.State.AttachedHandlerField?.Invoke((IAccessorMemberInfo)s.member, (TTarget)t, s.metadata);
                    return new AutoProperty(s.member.State.PropertyChanged,
                        s.member.State.GetDefaultValue == null ? s.member.State.DefaultValueField : s.member.State.GetDefaultValue((IAccessorMemberInfo)s.member, (TTarget)t));
                });
            }

            public void SetValue(IAccessorMemberInfo member, TTarget target, TValue value, IReadOnlyMetadataContext? metadata)
            {
                var oldValue = Value;
                if (EqualityComparer<TValue>.Default.Equals(oldValue, value))
                    return;
                Value = value;
                _propertyChanged?.Invoke(member, target, oldValue, value, metadata);
                Raise(target, metadata, metadata);
            }

            #endregion
        }

        #endregion
    }
}