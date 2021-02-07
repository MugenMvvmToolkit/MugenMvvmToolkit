using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Components
{
    public sealed class EventInfoMemberObserverProvider : IMemberObserverProviderComponent, IHasPriority
    {
        private static readonly MethodInfo RaiseMethod = typeof(BindingMugenExtensions).GetMethodOrThrow(nameof(BindingMugenExtensions.Raise), BindingFlagsEx.StaticPublic);

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly Func<object, EventInfo, EventListenerCollection?> _createWeakListenerDelegate;
        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> _memberObserverHandler;
        private readonly IReflectionManager? _reflectionManager;

        [Preserve(Conditional = true)]
        public EventInfoMemberObserverProvider(IAttachedValueManager? attachedValueManager = null, IReflectionManager? reflectionManager = null)
        {
            _attachedValueManager = attachedValueManager;
            _reflectionManager = reflectionManager;
            _createWeakListenerDelegate = CreateWeakListener;
            _memberObserverHandler = TryObserve;
        }

        public int Priority { get; set; } = ObservationComponentPriority.EventObserverProvider;

        public MemberObserver TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            if (member is EventInfo eventInfo)
                return TryGetMemberObserver(eventInfo);
            return default;
        }

        private ActionToken TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            var tuple = (Tuple<EventInfo, string>) member;
            if (target == null && !tuple.Item1.IsStatic())
                return default;

            var listenerInternal = (target ?? tuple.Item1.DeclaringType!).AttachedValues(metadata, _attachedValueManager)
                                                                         .GetOrAdd(tuple.Item2, tuple.Item1, _createWeakListenerDelegate);
            if (listenerInternal == null)
                return default;
            return listenerInternal.Add(listener);
        }

        private MemberObserver TryGetMemberObserver(EventInfo member)
        {
            if (member.EventHandlerType == typeof(EventHandler) || member.EventHandlerType != null && member.EventHandlerType.CanCreateDelegate(RaiseMethod, _reflectionManager))
                return new MemberObserver(_memberObserverHandler, Tuple.Create(member, BindingInternalConstant.EventPrefixObserverMember + member.Name));
            return default;
        }

        private EventListenerCollection? CreateWeakListener(object? target, EventInfo eventInfo)
        {
            var listenerInternal = new EventListenerCollection();
            var handler = eventInfo.EventHandlerType == typeof(EventHandler)
                ? new EventHandler(listenerInternal.Raise)
                : eventInfo.EventHandlerType!.TryCreateDelegate(listenerInternal, RaiseMethod, _reflectionManager);

            if (handler == null)
                return null;

            var addMethod = eventInfo.GetAddMethod(true);
            if (addMethod == null)
                return null;

            if (eventInfo.IsStatic())
                addMethod.GetMethodInvoker<Action<Delegate>>(_reflectionManager).Invoke(handler);
            else
                addMethod.GetMethodInvoker<Action<object, Delegate>>(_reflectionManager).Invoke(target!, handler);
            return listenerInternal;
        }
    }
}