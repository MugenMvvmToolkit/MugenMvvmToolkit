using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
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
        internal bool HasDefaultValueField;

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
            DefaultValueField = default!;
            HasDefaultValueField = false;
            PropertyType = propertyType;
            UnderlyingMemberField = null;
        }

        #endregion

        #region Methods

        public PropertyBuilder<TTarget, TValue> Static()
        {
            Should.BeSupported(AttachedHandlerField == null, nameof(AttachedHandler));
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

        public PropertyBuilder<TTarget, TValue> AttachedHandler(MemberAttachedDelegate<IAccessorMemberInfo, TTarget> attachedHandler)
        {
            Should.NotBeNull(attachedHandler, nameof(attachedHandler));
            Should.BeSupported(!IsStatic, nameof(Static));
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
            var id = GenerateMemberId(true);
            if (IsStatic)
            {
                return Property((id, PropertyChanged, HasDefaultValueField, DefaultValueField, GetDefaultValue), (member, target, metadata) =>
                {
                    if (!AttachedMemberBuilder.TryGetStaticValue<TValue>(member.State.id, out var value) && member.State.HasDefaultValueField)
                    {
                        value = member.State.GetDefaultValue == null ? member.State.DefaultValueField : member.State.GetDefaultValue(member, target);
                        AttachedMemberBuilder.TrySetStaticValue(member.State.id, value, out _);
                    }

                    return value;
                }, (member, target, value, metadata) =>
                {
                    if (!AttachedMemberBuilder.TrySetStaticValue(member.State.id, value, out var oldValue))
                        return;
                    member.State.PropertyChanged?.Invoke(member, target, oldValue, value, metadata);
                    AttachedMemberBuilder.RaiseStaticEvent(member.State.id, metadata, metadata);
                }, (member, target, listener, metadata) => AttachedMemberBuilder.AddStaticEvent(member.State.id, listener), null);
            }

            if (IsInherits)
            {
                return Property((id, AttachedHandlerField, PropertyChanged, DefaultValueField, GetDefaultValue),
                    (member, target, metadata) => InheritedProperty.GetOrAdd(target, member, metadata).Value,
                    (member, target, value, metadata) => InheritedProperty.GetOrAdd(target, member, metadata).SetValue(value, metadata),
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
                AttachedMemberBuilder.GetFlags(IsStatic), UnderlyingMemberField, state, getValue, setValue, tryObserve, raise);
        }

        #endregion

        #region Nested types

        private sealed class InheritedProperty : EventListenerCollection, IValueHolder<IWeakReference>, IEventListener
        {
            #region Fields

            private readonly DelegateObservableMemberInfo<TTarget, (string id, MemberAttachedDelegate<IAccessorMemberInfo, TTarget>? AttachedHandlerField, ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>?
                PropertyChanged, TValue DefaultValueField, Func<IAccessorMemberInfo, TTarget, TValue>? GetDefaultValue)> _member;

            [NotNull]
            private readonly TTarget _target;

            private IWeakReference? _parentRef;
            private ActionToken _parentToken;

            private int _state;
            private ActionToken _valueToken;
            public TValue Value;

            private const int DefaultState = 0;
            private const int ParentState = 1;
            private const int HasValueState = 2;

            #endregion

            #region Constructors

            private InheritedProperty(TTarget target,
                DelegateObservableMemberInfo<TTarget, (string id, MemberAttachedDelegate<IAccessorMemberInfo, TTarget>? AttachedHandlerField, ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>? PropertyChanged,
                    TValue DefaultValueField, Func<IAccessorMemberInfo, TTarget, TValue>? GetDefaultValue)> member, IReadOnlyMetadataContext? metadata)
            {
                _target = target;
                _member = member;
                Value = member.State.GetDefaultValue == null ? member.State.DefaultValueField : member.State.GetDefaultValue((IAccessorMemberInfo)member, target);
                InvalidateParent(metadata);
            }

            #endregion

            #region Properties

            IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

            #endregion

            #region Implementation of interfaces

            bool IEventListener.TryHandle<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata)
            {
                if (!Default.IsValueType<T>() && message is InheritedProperty inheritedProperty)
                    ApplyValues(inheritedProperty, metadata);
                else
                    InvalidateParent(metadata);
                return true;
            }

            #endregion

            #region Methods

            public static InheritedProperty GetOrAdd(TTarget target,
                DelegateObservableMemberInfo<TTarget, (string id, MemberAttachedDelegate<IAccessorMemberInfo, TTarget>? AttachedHandlerField, ValueChangedDelegate<IAccessorMemberInfo, TTarget, TValue>? PropertyChanged,
                    TValue DefaultValueField, Func<IAccessorMemberInfo, TTarget, TValue>? GetDefaultValue)> member, IReadOnlyMetadataContext? metadata)
            {
                var attachedValueProvider = MugenService.AttachedValueProvider;
                if (attachedValueProvider.TryGet<InheritedProperty>(target!, member.State.id, out var value))
                    return value;
#pragma warning disable 8634
                return attachedValueProvider.GetOrAdd(target!, member.State.id, (member, metadata), (t, s) =>
                {
                    s.member.State.AttachedHandlerField?.Invoke((IAccessorMemberInfo)s.member, t, s.metadata);
                    return new InheritedProperty(t, s.member, s.metadata);
                });
#pragma warning restore 8634
            }

            public void SetValue(TValue value, IReadOnlyMetadataContext? metadata)
            {
                SetValue(value, HasValueState, metadata);
            }

            private void SetValue(TValue value, int state, IReadOnlyMetadataContext? metadata)
            {
                if (state == HasValueState)
                {
                    if (!_parentToken.IsEmpty)
                        _parentToken.Dispose();
                    if (_parentRef != null)
                    {
                        _valueToken.Dispose();
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
                _member.State.PropertyChanged?.Invoke((IAccessorMemberInfo)_member, _target, oldValue, value, metadata);
                Raise(_target, this, metadata);
            }

            private void InvalidateParent(IReadOnlyMetadataContext? metadata)
            {
                var member = MugenBindingService
                    .MemberManager
                    .GetMember(_target.GetType(), MemberType.Accessor, MemberFlags.InstanceAll, BindableMembers.Object.Parent.Name, metadata) as IAccessorMemberInfo;
                var parent = member?.GetValue(_target, metadata) as TTarget;
                if (ReferenceEquals(_parentRef?.Target, parent))
                {
                    if (member != null && _parentRef == null && _parentToken.IsEmpty)
                        _parentToken = member.TryObserve(_target, this, metadata);
                    return;
                }

                _valueToken.Dispose();
                if (member != null && _parentToken.IsEmpty)
                    _parentToken = member.TryObserve(_target, this, metadata);
                if (parent == null)
                {
                    _parentRef = null;
                    ApplyValues(null, metadata);
                    return;
                }

                _parentRef = parent.ToWeakReference();
                var inheritedProperty = GetOrAdd(parent, _member, metadata);
                ApplyValues(inheritedProperty, metadata);
                _valueToken = inheritedProperty.Add(this);
            }

            private void ApplyValues(InheritedProperty? parentProperty, IReadOnlyMetadataContext? metadata)
            {
                if (parentProperty != null && parentProperty._state != DefaultState)
                    SetValue(parentProperty.Value, ParentState, metadata);
                else if (_state == ParentState)
                    SetValue(_member.State.GetDefaultValue == null ? _member.State.DefaultValueField : _member.State.GetDefaultValue((IAccessorMemberInfo)_member, _target), DefaultState, metadata);
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
                var attachedValueProvider = MugenService.AttachedValueProvider;
                if (attachedValueProvider.TryGet<AutoProperty>(target!, member.State.id, out var value))
                    return value;
#pragma warning disable 8634
                return attachedValueProvider.GetOrAdd(target!, member.State.id, (member, metadata), (t, s) =>
                {
                    s.member.State.AttachedHandlerField?.Invoke((IAccessorMemberInfo)s.member, t, s.metadata);
                    return new AutoProperty(s.member.State.PropertyChanged, s.member.State.GetDefaultValue == null ? s.member.State.DefaultValueField : s.member.State.GetDefaultValue((IAccessorMemberInfo)s.member, t));
                });
#pragma warning restore 8634
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