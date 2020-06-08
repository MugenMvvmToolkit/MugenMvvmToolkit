using System;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members
{
    public sealed class EventMemberInfo : IObservableMemberInfo
    {
        #region Fields

        private readonly EventInfo _eventInfo;
        private readonly MemberObserver _observer;

        #endregion

        #region Constructors

        public EventMemberInfo(string name, EventInfo eventInfo, MemberObserver observer)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(eventInfo, nameof(eventInfo));
            _eventInfo = eventInfo;
            _observer = observer;
            Name = name;
            Type = _eventInfo.EventHandlerType;
            AccessModifiers = _eventInfo.GetAccessModifiers();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType => _eventInfo.DeclaringType;

        public Type Type { get; }

        public object? UnderlyingMember => _eventInfo;

        public MemberType MemberType => MemberType.Event;

        public MemberFlags AccessModifiers { get; }

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            return _observer.TryObserve(target, listener!, metadata);
        }

        #endregion
    }
}