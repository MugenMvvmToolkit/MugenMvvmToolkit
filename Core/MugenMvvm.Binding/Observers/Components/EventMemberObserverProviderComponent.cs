﻿using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class EventMemberObserverProviderComponent : IMemberObserverProviderComponent, MemberObserver.IHandler, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;
        private readonly FuncEx<MethodInfo, Type, IReadOnlyMetadataContext?, MemberObserver> _tryGetMemberObserverMethodDelegate;
        private readonly FuncEx<PropertyInfo, Type, IReadOnlyMetadataContext?, MemberObserver> _tryGetMemberObserverPropertyDelegate;
        private readonly FuncEx<MemberObserverRequest, Type, IReadOnlyMetadataContext?, MemberObserver> _tryGetMemberObserverRequestDelegate;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventMemberObserverProviderComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
            _tryGetMemberObserverMethodDelegate = TryGetMemberObserver;
            _tryGetMemberObserverPropertyDelegate = TryGetMemberObserver;
            _tryGetMemberObserverRequestDelegate = TryGetMemberObserver;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        public Func<Type, string, IReadOnlyMetadataContext?, IBindingEventInfo?>? EventFinder { get; set; }

        #endregion

        #region Implementation of interfaces

        Unsubscriber MemberObserver.IHandler.TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return ((IBindingEventInfo)member).TrySubscribe(target, listener, metadata);
        }

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (_tryGetMemberObserverPropertyDelegate is FuncEx<TMember, Type, IReadOnlyMetadataContext?, MemberObserver> provider1)
                return provider1.Invoke(member, type, metadata);
            if (_tryGetMemberObserverMethodDelegate is FuncEx<TMember, Type, IReadOnlyMetadataContext?, MemberObserver> provider2)
                return provider2.Invoke(member, type, metadata);
            return default;
        }

        #endregion

        #region Methods

        private MemberObserver TryGetMemberObserver(in MemberObserverRequest request, Type type, IReadOnlyMetadataContext? metadata)
        {
            if (request.Member is PropertyInfo p)
                return TryGetMemberObserver(p, type, metadata);
            if (request.Member is MethodInfo m)
                return TryGetMemberObserver(m, type, metadata);
            return default;
        }

        private MemberObserver TryGetMemberObserver(in MethodInfo member, Type type, IReadOnlyMetadataContext? metadata)
        {
            var observableMember = TryGetEvent(type, member.Name, member.GetAccessModifiers(), metadata);
            if (observableMember != null)
                return new MemberObserver(this, observableMember);

            return default;
        }

        private MemberObserver TryGetMemberObserver(in PropertyInfo member, Type type, IReadOnlyMetadataContext? metadata)
        {
            var observableMember = TryGetEvent(type, member.Name, (member.GetGetMethod(true) ?? member.GetSetMethod(true)).GetAccessModifiers(), metadata);
            if (observableMember != null)
                return new MemberObserver(this, observableMember);

            return default;
        }

        private IBindingEventInfo? TryGetEvent(Type type, string memberName, BindingMemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            if (EventFinder != null)
                return EventFinder(type, memberName, metadata);

            var provider = _memberProvider.ServiceIfNull();
            return provider.GetMember(type, memberName + BindingInternalConstants.ChangedEventPostfix, BindingMemberType.Event, flags, metadata) as IBindingEventInfo
                   ?? provider.GetMember(type, memberName + "Change", BindingMemberType.Event, flags, metadata) as IBindingEventInfo;
        }

        #endregion
    }
}