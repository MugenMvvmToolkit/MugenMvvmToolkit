using System;
using System.Reflection;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members
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
            Type = _eventInfo.EventHandlerType ?? typeof(EventHandler);
            AccessModifiers = _eventInfo.GetAccessModifiers();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType => _eventInfo.DeclaringType ?? typeof(object);

        public Type Type { get; }

        public object? UnderlyingMember => _eventInfo;

        public MemberType MemberType => MemberType.Event;

        public MemberFlags AccessModifiers { get; }

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null) => _observer.TryObserve(target, listener!, metadata);

        #endregion
    }
}