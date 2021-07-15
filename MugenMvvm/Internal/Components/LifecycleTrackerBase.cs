﻿using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public abstract class LifecycleTrackerBase<TOwner, T, TTarget> : ILifecycleTrackerComponent<TOwner, T>
        where TOwner : class, IComponentOwner
        where TTarget : class
        where T : class, IEnum
    {
        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly HashSet<T> _commonStates;

        protected LifecycleTrackerBase(IAttachedValueManager? attachedValueManager)
        {
            _commonStates = new HashSet<T>();
            Trackers = new List<Action<TTarget, HashSet<T>, T, IReadOnlyMetadataContext?>>(2);
            _attachedValueManager = attachedValueManager;
        }

        public List<Action<TTarget, HashSet<T>, T, IReadOnlyMetadataContext?>> Trackers { get; }

        protected IAttachedValueManager AttachedValueManager => _attachedValueManager.DefaultIfNull();

        protected virtual bool IsInState(TOwner owner, TTarget target, T state, IReadOnlyMetadataContext? metadata)
        {
            lock (_commonStates)
            {
                if (target.AttachedValues(metadata, _attachedValueManager).TryGet(InternalConstant.LifecycleListKey, out var value))
                    return ((HashSet<T>) value!).Contains(state);
            }

            return false;
        }

        protected virtual TTarget GetTarget(object target) => (TTarget) target;

        protected void OnLifecycleChanged(object targetRaw, T lifecycleState, IReadOnlyMetadataContext? metadata)
        {
            if (Trackers.Count == 0)
                return;

            var target = GetTarget(targetRaw);
            lock (_commonStates)
            {
                var attachedValues = target.AttachedValues(metadata, _attachedValueManager);
                attachedValues.TryGet(InternalConstant.LifecycleListKey, out var value);
                if (value is not HashSet<T> states)
                {
                    _commonStates.Clear();
                    states = _commonStates;
                }

                for (var i = 0; i < Trackers.Count; i++)
                    Trackers[i].Invoke(target, states, lifecycleState, metadata);

                if (ReferenceEquals(states, _commonStates) && _commonStates.Count != 0)
                {
                    states = new HashSet<T>(_commonStates);
                    _commonStates.Clear();
                    attachedValues.Set(InternalConstant.LifecycleListKey, states, out _);
                }
            }
        }

        bool ILifecycleTrackerComponent<TOwner, T>.IsInState(TOwner owner, object target, T state, IReadOnlyMetadataContext? metadata) =>
            IsInState(owner, GetTarget(target), state, metadata);
    }
}