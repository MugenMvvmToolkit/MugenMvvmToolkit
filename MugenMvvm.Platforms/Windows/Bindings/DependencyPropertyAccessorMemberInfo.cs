using System;
using System.Windows;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Windows.Bindings
{
    public sealed class DependencyPropertyAccessorMemberInfo : IAccessorMemberInfo, IHasPriority
    {
        private readonly DependencyProperty _dependencyProperty;
        private MemberObserver _observer;

        public DependencyPropertyAccessorMemberInfo(DependencyProperty dependencyProperty, string name, Type reflectedType, EnumFlags<MemberFlags> flags)
        {
            Should.NotBeNull(dependencyProperty, nameof(dependencyProperty));
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _dependencyProperty = dependencyProperty;
            Name = name;
            DeclaringType = reflectedType;
            MemberFlags = flags;
        }

        public bool CanRead => true;

        public bool CanWrite => !_dependencyProperty.ReadOnly;

        public string Name { get; }

        public Type DeclaringType { get; }

        public Type Type => _dependencyProperty.PropertyType;

        public object UnderlyingMember => _dependencyProperty;

        public MemberType MemberType => MemberType.Accessor;

        public EnumFlags<MemberFlags> MemberFlags { get; }

        int IHasPriority.Priority => 1;

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            var value = ((DependencyObject)target!).GetValue(_dependencyProperty);
            return DependencyProperty.UnsetValue == value ? BindingMetadata.UnsetValue : value;
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null) => ((DependencyObject)target!).SetValue(_dependencyProperty, value);

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer.IsEmpty)
                _observer = MugenService.ObservationManager.TryGetMemberObserver(DeclaringType, this, metadata).NoDoIfEmpty();
            return _observer.TryObserve(target, listener, metadata);
        }
    }
}