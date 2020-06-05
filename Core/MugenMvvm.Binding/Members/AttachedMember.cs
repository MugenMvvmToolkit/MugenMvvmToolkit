using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Members
{
    public static class AttachedMember
    {
        #region Fields

        private static readonly Dictionary<string, EventListenerCollection> StaticEvents = new Dictionary<string, EventListenerCollection>();

        #endregion

        #region Methods

        public static DelegateEventMemberInfo<TTarget, TState> Event<TTarget, TState>(BindableEventDescriptor<TTarget> descriptor, TState state, DelegateEventMemberInfo<TTarget, TState>.TrySubscribeDelegate trySubscribe,
            Action<TTarget, IEventInfo>? eventAttachedHandler, DelegateEventMemberInfo<TTarget, TState>.RaiseDelegate? raise = null, Type? eventType = null, IAttachedValueProvider? attachedValueProvider = null)
            where TTarget : class
        {
            return Event(descriptor, typeof(TTarget), state, trySubscribe, eventAttachedHandler, raise, eventType, attachedValueProvider);
        }

        public static DelegateEventMemberInfo<TTarget, TState> Event<TTarget, TState>(string name, Type declaringType, TState state, DelegateEventMemberInfo<TTarget, TState>.TrySubscribeDelegate trySubscribe,
            Action<TTarget, IEventInfo>? eventAttachedHandler, DelegateEventMemberInfo<TTarget, TState>.RaiseDelegate? raise = null, Type? eventType = null, IAttachedValueProvider? attachedValueProvider = null)
            where TTarget : class
        {
            Should.NotBeNull(trySubscribe, nameof(trySubscribe));
            Should.NotBeNull(eventAttachedHandler, nameof(eventAttachedHandler));
            var id = GenerateMemberId(BindingInternalConstant.AttachedMemberHandlerPrefix, declaringType, name);
            var original = trySubscribe;
            trySubscribe = (member, target, listener, metadata) =>
            {
                RaiseMemberAttached(attachedValueProvider.DefaultIfNull(), id, target, member, eventAttachedHandler);
                return original(member, target, listener, metadata);
            };

            return Event(name, declaringType, state, trySubscribe, raise, eventType);
        }

        public static DelegateEventMemberInfo<TTarget, (string, string?, Action<TTarget, IEventInfo>?, IAttachedValueProvider?)> Event<TTarget>(BindableEventDescriptor<TTarget> descriptor,
            Action<TTarget, IEventInfo>? eventAttachedHandler = null, Type? eventType = null, IAttachedValueProvider? attachedValueProvider = null)
            where TTarget : class
        {
            return Event(descriptor, typeof(TTarget), eventAttachedHandler, eventType, attachedValueProvider);
        }

        public static DelegateEventMemberInfo<TTarget, (string, string?, Action<TTarget, IEventInfo>?, IAttachedValueProvider?)> Event<TTarget>(string name, Type declaringType,
            Action<TTarget, IEventInfo>? eventAttachedHandler = null, Type? eventType = null, IAttachedValueProvider? attachedValueProvider = null)
            where TTarget : class
        {
            var id = GenerateMemberId(BindingInternalConstant.AttachedEventPrefix, declaringType, name);
            string? attachedId = null;
            if (eventAttachedHandler != null)
                attachedId = GenerateMemberId(BindingInternalConstant.AttachedMemberHandlerPrefix, declaringType, name);

            return Event<TTarget, (string id, string? attachedId, Action<TTarget, IEventInfo>? eventAttachedHandler, IAttachedValueProvider? attachedValueProvider)>(name,
                declaringType, (id, attachedId, eventAttachedHandler, attachedValueProvider), (member, target, listener, metadata) =>
                {
                    if (member.State.eventAttachedHandler != null)
                        RaiseMemberAttached(member.State.attachedValueProvider, member.State.attachedId!, target, member, member.State.eventAttachedHandler);
                    return EventListenerCollection.GetOrAdd(target!, member.State.id, member.State.attachedValueProvider).Add(listener);
                }, (member, target, message, metadata) => { EventListenerCollection.Raise(target!, member.State.id, message, member.State.attachedValueProvider); }, eventType);
        }

        public static DelegateEventMemberInfo<object?, string> EventStatic(string name, Type declaringType, Type? eventType = null)
        {
            return Event<object?, string>(name, declaringType, GenerateMemberId(BindingInternalConstant.AttachedEventPrefix, declaringType, name), (member, target, listener, metadata) =>
            {
                EventListenerCollection events;
                lock (StaticEvents)
                {
                    if (!StaticEvents.TryGetValue(member.State, out events))
                    {
                        events = new EventListenerCollection();
                        StaticEvents[member.State] = events;
                    }
                }

                return events.Add(listener);
            }, (member, target, message, metadata) =>
            {
                lock (StaticEvents)
                {
                    if (StaticEvents.TryGetValue(member.State, out var listener))
                        listener.Raise(null, message);
                }
            }, eventType, MemberFlags.StaticPublic);
        }

        public static DelegateEventMemberInfo<TTarget, TState> Event<TTarget, TState>(string name, Type declaringType, TState state, DelegateEventMemberInfo<TTarget, TState>.TrySubscribeDelegate trySubscribe,
            DelegateEventMemberInfo<TTarget, TState>.RaiseDelegate? raise = null, Type? eventType = null, MemberFlags accessModifiers = MemberFlags.InstancePublic)
            where TTarget : class?
        {
            return new DelegateEventMemberInfo<TTarget, TState>(name, declaringType, state, trySubscribe, raise, eventType, accessModifiers);
        }

        public static void RaiseMemberAttached<TTarget, TMember>(IAttachedValueProvider? attachedValueProvider, string id, TTarget target, TMember member, Action<TTarget, TMember> handler)
            where TTarget : class
            where TMember : class, IMemberInfo
        {
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(handler, nameof(handler));
            attachedValueProvider.DefaultIfNull().GetOrAdd(target, id, (member, handler), (t, state) =>
            {
                state.handler(t, state.member);
                return (object?) null;
            });
        }

        private static string GenerateMemberId(string prefix, Type declaringType, string name)
        {
            return prefix + declaringType.Name + name;
        }

        #endregion
    }
}