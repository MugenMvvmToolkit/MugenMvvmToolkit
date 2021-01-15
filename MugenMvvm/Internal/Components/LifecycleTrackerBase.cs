using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public abstract class LifecycleTrackerBase<T, TTarget> : ILifecycleTrackerComponent<T> where TTarget : class where T : class, IEnum
    {
        private readonly HashSet<T> _commonStates;

        protected LifecycleTrackerBase()
        {
            _commonStates = new HashSet<T>();
            Trackers = new List<Action<TTarget, HashSet<T>, T, IReadOnlyMetadataContext?>>(2);
        }

        public List<Action<TTarget, HashSet<T>, T, IReadOnlyMetadataContext?>> Trackers { get; }

        protected virtual bool IsInState(object owner, TTarget target, T state, IReadOnlyMetadataContext? metadata)
        {
            lock (_commonStates)
            {
                if (target.AttachedValues(metadata).TryGet(InternalConstant.LifecycleListKey, out var value))
                    return ((HashSet<T>) value!).Contains(state);
            }

            return false;
        }

        protected virtual TTarget GetTarget(TTarget target) => target;

        protected void OnLifecycleChanged(TTarget target, T lifecycleState, IReadOnlyMetadataContext? metadata)
        {
            if (Trackers.Count == 0)
                return;

            lock (_commonStates)
            {
                var attachedValues = target.AttachedValues();
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

        bool ILifecycleTrackerComponent<T>.IsInState(object owner, object target, T state, IReadOnlyMetadataContext? metadata) =>
            IsInState(owner, (TTarget) target, state, metadata);
    }
}