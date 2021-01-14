﻿using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation
{
    public sealed class RootSourceObserver : EventListenerCollection, IWeakEventListener
    {
        private readonly IWeakReference _targetRef;
        private IWeakReference? _parentRef;
        private ActionToken _parentToken;

        private RootSourceObserver(object target)
        {
            Should.NotBeNull(target, nameof(target));
            _targetRef = target.ToWeakReference();
        }

        public bool IsWeak => true;

        public bool IsAlive => _targetRef.IsAlive;

        public static RootSourceObserver GetOrAdd(object target) =>
            target.AttachedValues().GetOrAdd(BindingInternalConstant.RootObserver, target, (o, _) => new RootSourceObserver(o));

        public static void Clear(object target)
        {
            if (target.AttachedValues().Remove(BindingInternalConstant.RootObserver, out var value))
                (value as RootSourceObserver)?.Dispose();
        }

        public void Dispose()
        {
            ClearParent();
            Clear();
        }

        public object? Get(IReadOnlyMetadataContext? metadata) => Get(_targetRef.Target, metadata);

        [return: NotNullIfNotNull("target")]
        public object? Get(object? target, IReadOnlyMetadataContext? metadata) => BindingMugenExtensions.GetRoot(target, metadata);

        protected override void OnListenersAdded()
        {
            var target = _targetRef.Target;
            if (target != null)
                UpdateParent(target, null);
        }

        protected override void OnListenersRemoved() => ClearParent();

        private bool UpdateParent(object target, IReadOnlyMetadataContext? metadata)
        {
            var member = BindableMembers.For<object>().Parent().TryGetMember(target.GetType(), MemberFlags.InstancePublicAll, metadata);
            var oldParent = _parentRef?.Target;
            var parent = member?.GetValue(target, metadata);
            if (oldParent == parent)
            {
                if (member != null && _parentRef == null && _parentToken.IsEmpty)
                    _parentToken = member.TryObserve(target, this, metadata);
                return false;
            }

            if (member != null && _parentToken.IsEmpty)
                _parentToken = member.TryObserve(target, this, metadata);
            if (oldParent != null)
                GetOrAdd(oldParent).Remove(this);
            if (parent != null)
                GetOrAdd(parent).Add(this);
            _parentRef = parent?.ToWeakReference();
            return true;
        }

        private void ClearParent()
        {
            _parentToken.Dispose();
            var parent = _parentRef?.Target;
            if (parent != null)
            {
                GetOrAdd(parent).Remove(this);
                _parentRef = WeakReferenceImpl.Empty;
            }
        }

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                var target = _targetRef.Target;
                if (target == null)
                    return false;
                if (message is RootSourceObserver)
                    Raise(target, message, metadata);
                else if (UpdateParent(target, metadata))
                    Raise(target, this, metadata);
            }
            catch (Exception e)
            {
                MugenService.Application.OnUnhandledException(e, UnhandledExceptionType.Binding, metadata);
            }

            return true;
        }
    }
}