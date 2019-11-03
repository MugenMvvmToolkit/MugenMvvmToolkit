﻿using System;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class BindingEventInfo : IBindingEventInfo
    {
        #region Fields

        private readonly EventInfo _eventInfo;
        private readonly MemberObserver _observer;

        #endregion

        #region Constructors

        public BindingEventInfo(string name, EventInfo eventInfo, MemberObserver observer)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(eventInfo, nameof(eventInfo));
            _eventInfo = eventInfo;
            _observer = observer;
            Name = name;
            Type = _eventInfo.EventHandlerType;
            AccessModifiers = (_eventInfo.GetAddMethod(true) ?? _eventInfo.GetRemoveMethod(true)).GetAccessModifiers();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type Type { get; }

        public object? Member => _eventInfo;

        public BindingMemberType MemberType => BindingMemberType.Event;

        public BindingMemberFlags AccessModifiers { get; }

        #endregion

        #region Implementation of interfaces

        public Unsubscriber TrySubscribe(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            return _observer.TryObserve(target, listener!, metadata);
        }

        #endregion
    }
}