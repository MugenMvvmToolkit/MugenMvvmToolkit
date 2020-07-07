using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observation.Components
{
    public sealed class EventInfoMemberObserverProvider : IMemberObserverProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly Func<object, EventInfo, EventListenerCollection?> _createWeakListenerDelegate;
        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> _memberObserverHandler;
        private readonly IReflectionManager? _reflectionManager;

        private static readonly MethodInfo RaiseMethod = typeof(MugenBindingExtensions)
            .GetMethodOrThrow(nameof(MugenBindingExtensions.Raise), BindingFlagsEx.StaticPublic);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventInfoMemberObserverProvider(IAttachedValueManager? attachedValueManager = null, IReflectionManager? reflectionManager = null)
        {
            _attachedValueManager = attachedValueManager;
            _reflectionManager = reflectionManager;
            _createWeakListenerDelegate = CreateWeakListener;
            _memberObserverHandler = TryObserve;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.Event;

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver<TMember>(IObservationManager observationManager, Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TMember>())
                return default;
            if (member is EventInfo eventInfo)
                return TryGetMemberObserver(eventInfo);
            return default;
        }

        #endregion

        #region Methods

        private ActionToken TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            var eventInfo = (EventInfo)member;
            if (target == null && !eventInfo.IsStatic())
                return default;

            var listenerInternal = _attachedValueManager
                .DefaultIfNull()
                .GetOrAdd(target ?? eventInfo.DeclaringType, BindingInternalConstant.EventPrefixObserverMember + eventInfo.Name, eventInfo, _createWeakListenerDelegate);
            if (listenerInternal == null)
                return default;
            return listenerInternal.Add(listener);
        }

        private MemberObserver TryGetMemberObserver(EventInfo member)
        {
            if (member.EventHandlerType == typeof(EventHandler) || member.EventHandlerType.CanCreateDelegate(RaiseMethod, _reflectionManager))
                return new MemberObserver(_memberObserverHandler, member);
            return default;
        }

        private EventListenerCollection? CreateWeakListener(object? target, EventInfo eventInfo)
        {
            var listenerInternal = new EventListenerCollection();
            var handler = eventInfo.EventHandlerType == typeof(EventHandler)
                ? new EventHandler(listenerInternal.Raise)
                : eventInfo.EventHandlerType.TryCreateDelegate(listenerInternal, RaiseMethod, _reflectionManager);

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

        #endregion
    }
}