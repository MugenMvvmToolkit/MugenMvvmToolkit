﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Entities
{
    public sealed class EntityTrackingCollection : ComponentOwnerBase<IEntityTrackingCollection>, IEntityTrackingCollection
    {
        #region Fields

        private readonly EntityStateDictionary _dictionary;

        #endregion

        #region Constructors

        public EntityTrackingCollection(IEqualityComparer<object>? comparer = null, IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _dictionary = new EntityStateDictionary(comparer);
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                lock (_dictionary)
                {
                    return _dictionary.Count;
                }
            }
        }

        public bool HasChanges
        {
            get
            {
                lock (_dictionary)
                {
                    foreach (var item in _dictionary)
                    {
                        if (item.Value != EntityState.Unchanged)
                            return true;
                    }

                    return false;
                }
            }
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<TrackingEntity> GetChanges<TState>(in TState state, Func<TrackingEntity, TState, bool> predicate)
        {
            Should.NotBeNull(predicate, nameof(predicate));
            LazyList<TrackingEntity> list = default;
            lock (_dictionary)
            {
                foreach (var pair in _dictionary)
                {
                    var entity = new TrackingEntity(pair.Key, pair.Value);
                    if (predicate(entity, state))
                        list.Add(entity);
                }
            }

            return (IReadOnlyList<TrackingEntity>?) list.List ?? Default.Array<TrackingEntity>();
        }

        public EntityState GetState(object entity, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(entity, nameof(entity));
            lock (_dictionary)
            {
                return _dictionary.TryGetValue(entity, out var state) ? state : EntityState.Detached;
            }
        }

        public void SetState(object entity, EntityState state, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(entity, nameof(entity));
            EntityState? oldState;
            lock (_dictionary)
            {
                if (!_dictionary.TryGetValue(entity, out oldState))
                    oldState = EntityState.Detached;
                state = GetComponents<IEntityStateChangingListener>().OnEntityStateChanging(this, entity, oldState, state, metadata);
                _dictionary.Remove(entity);
                if (state != EntityState.Detached)
                    _dictionary[entity] = state;
            }

            if (oldState != state)
                GetComponents<IEntityStateChangedListener>().OnEntityStateChanged(this, entity, oldState, state, metadata);
        }

        public void Clear(IReadOnlyMetadataContext? metadata = null)
        {
            lock (_dictionary)
            {
                if (_dictionary.Count != 0)
                {
                    var listeners = GetComponents<IEntityStateChangedListener>();
                    foreach (var pair in _dictionary)
                        listeners.OnEntityStateChanged(this, pair.Key, pair.Value, EntityState.Detached, metadata);
                }

                _dictionary.Clear();
            }
        }

        public IEnumerator<TrackingEntity> GetEnumerator()
        {
            lock (_dictionary)
            {
                var entities = new TrackingEntity[_dictionary.Count];
                var index = 0;
                foreach (var pair in _dictionary)
                    entities[index++] = new TrackingEntity(pair.Key, pair.Value);
                return ((IEnumerable<TrackingEntity>) entities).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Nested types

        private sealed class EntityStateDictionary : LightDictionary<object, EntityState>
        {
            #region Fields

            private readonly IEqualityComparer<object>? _comparer;

            #endregion

            #region Constructors

            public EntityStateDictionary(IEqualityComparer<object>? comparer)
            {
                _comparer = comparer;
            }

            #endregion

            #region Methods

            protected override int GetHashCode(object key)
            {
                if (_comparer == null)
                    return RuntimeHelpers.GetHashCode(key);
                return _comparer.GetHashCode(key);
            }

            protected override bool Equals(object x, object y)
            {
                if (_comparer == null)
                    return ReferenceEquals(x, y);
                return _comparer.Equals(x, y);
            }

            #endregion
        }

        #endregion
    }
}