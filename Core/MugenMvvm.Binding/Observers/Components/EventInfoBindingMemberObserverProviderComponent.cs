using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class EventInfoBindingMemberObserverProviderComponent : IBindingMemberObserverProviderComponent<EventInfo>, BindingMemberObserver.IHandler, IHasPriority
    {
        #region Fields

        private readonly IAttachedDictionaryProvider? _attachedDictionaryProvider;

        private static readonly MethodInfo RaiseMethod = GetRaiseMethod();
        private static readonly Func<object, EventInfo, object?, BindingEventListenerCollection?> CreateWeakListenerDelegate = CreateWeakListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventInfoBindingMemberObserverProviderComponent(IAttachedDictionaryProvider? attachedDictionaryProvider = null)
        {
            _attachedDictionaryProvider = attachedDictionaryProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        #endregion

        #region Implementation of interfaces

        IDisposable? BindingMemberObserver.IHandler.TryObserve(object? source, object member, IBindingEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (source == null)
                return null;

            var eventInfo = (EventInfo)member;
            var listenerInternal = _attachedDictionaryProvider
                .ServiceIfNull()
                .GetOrAdd(source, BindingInternalConstants.EventPrefixObserverMember + eventInfo.Name, eventInfo, null, CreateWeakListenerDelegate);
            return listenerInternal?.AddWithUnsubscriber(listener);
        }

        public BindingMemberObserver TryGetMemberObserver(Type type, in EventInfo member, IReadOnlyMetadataContext? metadata)
        {
            if (member.EventHandlerType.CanCreateDelegate(RaiseMethod))
                return new BindingMemberObserver(this, member);
            return default;
        }

        #endregion

        #region Methods

        private static MethodInfo GetRaiseMethod()
        {
            var m = typeof(BindingEventListenerCollection)
                .GetMethodUnified(nameof(BindingEventListenerCollection.Raise), MemberFlags.Public | MemberFlags.Instance);
            if (m == null)
                BindingExceptionManager.ThrowInvalidBindingMember(typeof(BindingEventListenerCollection), nameof(BindingEventListenerCollection.Raise));
            return m!;
        }

        private static BindingEventListenerCollection? CreateWeakListener(object target, EventInfo eventInfo, object? _)
        {
            var listenerInternal = new BindingEventListenerCollection();
            var handler = eventInfo.EventHandlerType.EqualsEx(typeof(EventHandler))
                ? new EventHandler(listenerInternal.Raise)
                : eventInfo.EventHandlerType.TryCreateDelegate(listenerInternal, RaiseMethod);

            if (handler == null)
                return null;

            var addMethod = eventInfo.GetAddMethodUnified(true);
            if (addMethod == null)
                return null;

            addMethod.InvokeEx(target, handler);
            return listenerInternal;
        }

        #endregion
    }
}