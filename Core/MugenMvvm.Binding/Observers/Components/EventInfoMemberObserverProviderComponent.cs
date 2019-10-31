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

        private readonly Func<object, EventInfo, object?, EventListenerCollection?> _createWeakListenerDelegate;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;

        private readonly FuncEx<EventInfo, Type, IReadOnlyMetadataContext?, MemberObserver> _tryGetMemberObserverEventDelegate;

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
            _createWeakListenerDelegate = CreateWeakListener;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        #endregion

        #region Implementation of interfaces

        Unsubscriber MemberObserver.IHandler.TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return default;

            var eventInfo = (EventInfo)member;
            var listenerInternal = _attachedValueManager
                .ServiceIfNull()
                .GetOrAdd(target, BindingInternalConstants.EventPrefixObserverMember + eventInfo.Name, eventInfo, null, _createWeakListenerDelegate);
            if (listenerInternal == null)
                return default;
            return listenerInternal.Add(listener);
        }

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (_tryGetMemberObserverEventDelegate is FuncEx<TMember, Type, IReadOnlyMetadataContext?, MemberObserver> provider)
                return provider.Invoke(member, type, metadata);
            return default;
        }

        #endregion

        #region Methods

        private MemberObserver TryGetMemberObserver(in EventInfo member, Type type, IReadOnlyMetadataContext? metadata)
        {
            if (member.EventHandlerType.CanCreateDelegate(RaiseMethod, _reflectionDelegateProvider))
                return new MemberObserver(this, member);
            return default;
        }

        private EventListenerCollection? CreateWeakListener(object target, EventInfo eventInfo, object? _)
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