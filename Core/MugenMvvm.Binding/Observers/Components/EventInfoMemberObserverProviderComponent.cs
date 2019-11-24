using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class EventInfoMemberObserverProviderComponent : IMemberObserverProviderComponent, MemberObserver.IHandler, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly Func<object, EventInfo, EventListenerCollection?> _createWeakListenerDelegate;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly FuncEx<EventInfo, MemberObserver> _tryGetMemberObserverEventDelegate;
        private readonly FuncEx<MemberObserverRequest, MemberObserver> _tryGetMemberObserverRequestDelegate;

        private static readonly MethodInfo RaiseMethod = typeof(EventListenerCollection)
            .GetMethodOrThrow(nameof(EventListenerCollection.Raise), BindingFlagsEx.InstancePublic);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventInfoMemberObserverProviderComponent(IAttachedValueManager? attachedValueManager = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _attachedValueManager = attachedValueManager;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _tryGetMemberObserverEventDelegate = TryGetMemberObserver;
            _tryGetMemberObserverRequestDelegate = TryGetMemberObserver;
            _createWeakListenerDelegate = CreateWeakListener;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.Event;

        #endregion

        #region Implementation of interfaces

        ActionToken MemberObserver.IHandler.TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return default;

            var eventInfo = (EventInfo)member;
            var listenerInternal = _attachedValueManager
                .DefaultIfNull()
                .GetOrAdd(target, BindingInternalConstant.EventPrefixObserverMember + eventInfo.Name, eventInfo, _createWeakListenerDelegate);
            if (listenerInternal == null)
                return default;
            return listenerInternal.Add(listener);
        }

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (_tryGetMemberObserverEventDelegate is FuncEx<TMember, MemberObserver> provider1)
                return provider1.Invoke(member);
            if (_tryGetMemberObserverRequestDelegate is FuncEx<TMember, MemberObserver> provider2)
                return provider2.Invoke(member);
            return default;
        }

        #endregion

        #region Methods

        private MemberObserver TryGetMemberObserver(in MemberObserverRequest request)
        {
            if (request.ReflectionMember is EventInfo eventInfo)
                return TryGetMemberObserver(eventInfo);
            return default;
        }

        private MemberObserver TryGetMemberObserver(in EventInfo member)
        {
            if (member.EventHandlerType.CanCreateDelegate(RaiseMethod, _reflectionDelegateProvider))
                return new MemberObserver(this, member);
            return default;
        }

        private EventListenerCollection? CreateWeakListener(object target, EventInfo eventInfo)
        {
            var listenerInternal = new EventListenerCollection();
            var handler = eventInfo.EventHandlerType == typeof(EventHandler)
                ? new EventHandler(listenerInternal.Raise)
                : eventInfo.EventHandlerType.TryCreateDelegate(listenerInternal, RaiseMethod, _reflectionDelegateProvider);

            if (handler == null)
                return null;

            var addMethod = eventInfo.GetAddMethod(true);
            if (addMethod == null)
                return null;

            addMethod.GetMethodInvoker<Action<object, Delegate>>(_reflectionDelegateProvider).Invoke(target, handler);
            return listenerInternal;
        }

        #endregion
    }
}