using System;
using System.Reflection;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members
{
    public sealed class EventMemberInfo : IObservableMemberInfo
    {
        private readonly EventInfo _eventInfo;
        private readonly ushort _modifiers;
        private readonly MemberObserver _observer;

        public EventMemberInfo(string name, EventInfo eventInfo, MemberObserver observer)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(eventInfo, nameof(eventInfo));
            _eventInfo = eventInfo;
            _observer = observer;
            Name = name;
            Type = _eventInfo.EventHandlerType ?? typeof(EventHandler);
            _modifiers = _eventInfo.GetAccessModifiers().Value();
        }

        public string Name { get; }

        public Type DeclaringType => _eventInfo.DeclaringType ?? typeof(object);

        public Type Type { get; }

        public object UnderlyingMember => _eventInfo;

        public MemberType MemberType => MemberType.Event;

        public EnumFlags<MemberFlags> MemberFlags => new(_modifiers);

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null) => _observer.TryObserve(target, listener!, metadata);
    }
}