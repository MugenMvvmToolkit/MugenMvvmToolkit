using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class EventInfoMemberObserverProviderComponent : IMemberObserverProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly Func<object, EventInfo, EventListenerCollection?> _createWeakListenerDelegate;
        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> _memberObserverHandler;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly FuncIn<MemberObserverRequest, MemberObserver> _tryGetMemberObserverRequestDelegate;

        private static readonly MethodInfo RaiseMethod = typeof(EventListenerCollection)
            .GetMethodOrThrow(nameof(EventListenerCollection.Raise), BindingFlagsEx.InstancePublic);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventInfoMemberObserverProviderComponent(IAttachedValueManager? attachedValueManager = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _attachedValueManager = attachedValueManager;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _tryGetMemberObserverRequestDelegate = TryGetMemberObserver;
            _createWeakListenerDelegate = CreateWeakListener;
            _memberObserverHandler = TryObserve;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.Event;

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TMember>())
            {
                if (_tryGetMemberObserverRequestDelegate is FuncIn<TMember, MemberObserver> provider)
                    return provider.Invoke(member);
            }
            else if (member is EventInfo eventInfo)
                return TryGetMemberObserver(eventInfo);
            return default;
        }

        #endregion

        #region Methods

        private ActionToken TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
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

        private MemberObserver TryGetMemberObserver(in MemberObserverRequest request)
        {
            if (request.ReflectionMember is EventInfo eventInfo)
                return TryGetMemberObserver(eventInfo);
            return default;
        }

        private MemberObserver TryGetMemberObserver(EventInfo member)
        {
            if (member.EventHandlerType.CanCreateDelegate(RaiseMethod, _reflectionDelegateProvider))
                return new MemberObserver(_memberObserverHandler, member);
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