﻿using System;
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

        private readonly IAttachedDictionaryProvider? _attachedDictionaryProvider;

        private readonly FuncEx<EventInfo, Type, IReadOnlyMetadataContext?, MemberObserver> _tryGetMemberObserverEventDelegate;

        private static readonly MethodInfo RaiseMethod = typeof(EventListenerCollection)
            .GetMethodOrThrow(nameof(EventListenerCollection.Raise), MemberFlags.Public | MemberFlags.Instance);

        private static readonly Func<object, EventInfo, object?, EventListenerCollection?> CreateWeakListenerDelegate = CreateWeakListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventInfoMemberObserverProviderComponent(IAttachedDictionaryProvider? attachedDictionaryProvider = null)
        {
            _attachedDictionaryProvider = attachedDictionaryProvider;
            _tryGetMemberObserverEventDelegate = TryGetMemberObserver;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        #endregion

        #region Implementation of interfaces

        IDisposable? MemberObserver.IHandler.TryObserve(object? source, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (source == null)
                return null;

            var eventInfo = (EventInfo) member;
            var listenerInternal = _attachedDictionaryProvider
                .ServiceIfNull()
                .GetOrAdd(source, BindingInternalConstants.EventPrefixObserverMember + eventInfo.Name, eventInfo, null, CreateWeakListenerDelegate);
            return listenerInternal?.AddWithUnsubscriber(listener);
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
            if (member.EventHandlerType.CanCreateDelegate(RaiseMethod))
                return new MemberObserver(this, member);
            return default;
        }

        private static EventListenerCollection? CreateWeakListener(object target, EventInfo eventInfo, object? _)
        {
            var listenerInternal = new EventListenerCollection();
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