using System;
using System.Reflection;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public sealed class EventInfoChildObserverProvider : IChildObserverProvider, IBindingMemberObserverCallback
    {
        #region Fields

        private readonly IAttachedValueProvider _attachedValueProvider;

        private static readonly MethodInfo RaiseMethod = typeof(BindingEventListenerCollection)
            .GetMethodUnified(nameof(BindingEventListenerCollection.Raise), MemberFlags.Public | MemberFlags.Instance);

        private static readonly Func<object, EventInfo, object, BindingEventListenerCollection> CreateWeakListenerDelegate = CreateWeakListener;

        #endregion

        #region Constructors

        public EventInfoChildObserverProvider(IAttachedValueProvider attachedValueProvider)
        {
            Should.NotBeNull(attachedValueProvider, nameof(attachedValueProvider));
            _attachedValueProvider = attachedValueProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IDisposable? IBindingMemberObserverCallback.TryObserve(object target, object member, IBindingEventListener listener, IReadOnlyMetadataContext metadata)
        {
            var eventInfo = (EventInfo) member;
            var listenerInternal = _attachedValueProvider.GetOrAdd(target, BindingInternalConstants.EventPrefixObserverMember + eventInfo.Name, eventInfo, null,
                CreateWeakListenerDelegate);
            if (listenerInternal.IsEmpty)
                return null;

            return listenerInternal.AddWithUnsubscriber(listener);
        }

        public bool TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata, out BindingMemberObserver observer)
        {
            if (member is EventInfo)
            {
                observer = new BindingMemberObserver(member, this);
                return true;
            }

            observer = default;
            return false;
        }

        #endregion

        #region Methods

        private static BindingEventListenerCollection CreateWeakListener(object target, EventInfo eventInfo, object _)
        {
            var listenerInternal = new BindingEventListenerCollection();
            Delegate handler = eventInfo.EventHandlerType.EqualsEx(typeof(EventHandler))
                ? new EventHandler(listenerInternal.Raise)
                : ToolkitServiceProvider.ReflectionManager.TryCreateDelegate(listenerInternal, RaiseMethod);

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