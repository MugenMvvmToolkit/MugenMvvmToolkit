using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class EventInfoBindingMemberObserverProviderComponent : IBindingMemberObserverProviderComponent, BindingMemberObserver.IHandler
    {
        #region Fields

        private readonly IAttachedDictionaryProvider _attachedDictionaryProvider;

        private static readonly MethodInfo RaiseMethod = GetRaiseMethod();
        private static readonly Func<object, EventInfo, object?, BindingEventListenerCollection?> CreateWeakListenerDelegate = CreateWeakListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventInfoBindingMemberObserverProviderComponent(IAttachedDictionaryProvider attachedDictionaryProvider)
        {
            Should.NotBeNull(attachedDictionaryProvider, nameof(attachedDictionaryProvider));
            _attachedDictionaryProvider = attachedDictionaryProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        #endregion

        #region Implementation of interfaces

        IDisposable? BindingMemberObserver.IHandler.TryObserve(object? target, object member, IBindingEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            var eventInfo = (EventInfo)member;
            var listenerInternal = _attachedDictionaryProvider
                .GetOrAdd(target!, BindingInternalConstants.EventPrefixObserverMember + eventInfo.Name, eventInfo, null, CreateWeakListenerDelegate);
            return listenerInternal?.AddWithUnsubscriber(listener);
        }

        public BindingMemberObserver TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            if (member is EventInfo eventInfo && eventInfo.EventHandlerType.CanCreateDelegate(RaiseMethod))
                return new BindingMemberObserver(this, member);
            return default;
        }

        int IComponent.GetPriority(object source)
        {
            return Priority;
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