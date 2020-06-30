using System;
using System.Collections.Generic;
using System.Globalization;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Builders
{
    public static class AttachedMemberBuilder
    {
        #region Fields

        private static readonly Dictionary<string, EventListenerCollection> StaticEvents = new Dictionary<string, EventListenerCollection>();
        private static readonly Dictionary<string, object?> StaticValues = new Dictionary<string, object?>();

        #endregion

        #region Methods

        public static EventBuilder<TTarget> GetBuilder<TTarget>(this BindableEventDescriptor<TTarget> descriptor, Type? eventType = null) where TTarget : class
        {
            return Event<TTarget>(descriptor.Name, null, eventType);
        }

        public static EventBuilder<TTarget> Event<TTarget>(string name, Type? declaringType = null, Type? eventType = null) where TTarget : class?
        {
            return new EventBuilder<TTarget>(name, declaringType ?? typeof(TTarget), eventType ?? typeof(EventHandler));
        }

        public static PropertyBuilder<TTarget, TValue> GetBuilder<TTarget, TValue>(this BindablePropertyDescriptor<TTarget, TValue> descriptor)
            where TTarget : class
        {
            return Property<TTarget, TValue>(descriptor.Name);
        }

        public static PropertyBuilder<TTarget, TValue> Property<TTarget, TValue>(string name, Type? declaringType = null, Type? propertyType = null)
            where TTarget : class?
        {
            return new PropertyBuilder<TTarget, TValue>(name, declaringType ?? typeof(TTarget), propertyType ?? typeof(TValue));
        }

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TReturn>(this BindableMethodDescriptor<TTarget, TReturn> descriptor) where TTarget : class
        {
            return Method<TTarget, TReturn>(descriptor.Name, typeof(TTarget), typeof(TReturn));
        }

        public static MethodBuilder<TTarget, TReturn> Method<TTarget, TReturn>(string name, Type? declaringType = null, Type? returnType = null) where TTarget : class?
        {
            return new MethodBuilder<TTarget, TReturn>(name, declaringType ?? typeof(TTarget), returnType ?? typeof(TReturn));
        }

        public static ParameterBuilder Parameter<TType>(string? name = null)
        {
            return new ParameterBuilder(name ?? "", typeof(TType));
        }

        public static ParameterBuilder Parameter(string name, Type type)
        {
            return new ParameterBuilder(name, type);
        }

        internal static void RaiseMemberAttached<TTarget, TMember>(string id, TTarget target, TMember member, MemberAttachedDelegate<TMember, TTarget> handler, IReadOnlyMetadataContext? metadata)
            where TTarget : class?
            where TMember : class, IMemberInfo
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(handler, nameof(handler));
            var attachedValueProvider = MugenService.AttachedValueProvider;
            if (!attachedValueProvider.Contains(target!, id))
            {
#pragma warning disable 8634
                attachedValueProvider.GetOrAdd(target, id, (member, handler, metadata), (t, state) =>
                {
                    state.handler(state.member, t, state.metadata);
                    return (object?)null;
                });
#pragma warning restore 8634
            }
        }

        internal static MemberFlags GetFlags(bool isStatic)
        {
            return isStatic ? MemberFlags.StaticPublic | MemberFlags.Attached : MemberFlags.InstancePublic | MemberFlags.Attached;
        }

        internal static string GenerateMemberId(string prefix, Type declaringType, string name)
        {
            return prefix + declaringType.FullName.Length.ToString(CultureInfo.InvariantCulture) + declaringType.Name + declaringType.AssemblyQualifiedName.Length.ToString(CultureInfo.InvariantCulture) + name;
        }

        internal static bool TryGetStaticValue<TValue>(string name, out TValue value)
        {
            lock (StaticValues)
            {
                if (StaticValues.TryGetValue(name, out var valueRaw))
                {
                    value = (TValue)valueRaw!;
                    return true;
                }

                value = default!;
                return false;
            }
        }

        internal static bool TrySetStaticValue<TValue>(string name, TValue value, out TValue oldValue)
        {
            lock (StaticValues)
            {
                if (StaticValues.TryGetValue(name, out var valueRaw))
                {
                    oldValue = (TValue)valueRaw!;
                    if (EqualityComparer<TValue>.Default.Equals(oldValue, value))
                        return false;
                }
                else
                    oldValue = default!;

                StaticValues[name] = BoxingExtensions.Box(value);
                return true;
            }
        }

        internal static ActionToken AddStaticEvent(string name, IEventListener listener)
        {
            EventListenerCollection events;
            lock (StaticEvents)
            {
                if (!StaticEvents.TryGetValue(name, out events))
                {
                    events = new EventListenerCollection();
                    StaticEvents[name] = events;
                }
            }

            return events.Add(listener);
        }

        internal static void RaiseStaticEvent<T>(string name, in T message, IReadOnlyMetadataContext? metadata)
        {
            lock (StaticEvents)
            {
                if (StaticEvents.TryGetValue(name, out var listener))
                    listener.Raise(null, message, metadata);
            }
        }

        #endregion
    }
}