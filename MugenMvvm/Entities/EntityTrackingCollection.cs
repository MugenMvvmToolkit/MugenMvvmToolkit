﻿using System;
using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
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

        private readonly Dictionary<object, EntityState> _dictionary;

        #endregion

        #region Constructors

        public EntityTrackingCollection(IEqualityComparer<object>? comparer = null, IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            _dictionary = new Dictionary<object, EntityState>(comparer ?? EqualityComparer<object>.Default);
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

        public IEqualityComparer<object> Comparer => _dictionary.Comparer;

        #endregion

        #region Implementation of interfaces

        public ItemOrIReadOnlyList<TrackingEntity> GetChanges<TState>(TState state, Func<TrackingEntity, TState, bool> predicate)
        {
            Should.NotBeNull(predicate, nameof(predicate));
            var editor = new ItemOrListEditor<TrackingEntity>();
            lock (_dictionary)
            {
                foreach (var pair in _dictionary)
                {
                    var entity = new TrackingEntity(pair.Key, pair.Value);
                    if (predicate(entity, state))
                        editor.Add(entity);
                }
            }

            return editor.ToItemOrList();
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
                state = GetComponents<IEntityStateChangingListener>(metadata).OnEntityStateChanging(this, entity, oldState, state, metadata);
                _dictionary.Remove(entity);
                if (state != EntityState.Detached)
                    _dictionary[entity] = state;
            }

            if (oldState != state)
                GetComponents<IEntityStateChangedListener>(metadata).OnEntityStateChanged(this, entity, oldState, state, metadata);
        }

        public void Clear(IReadOnlyMetadataContext? metadata = null)
        {
            lock (_dictionary)
            {
                if (_dictionary.Count != 0)
                {
                    var listeners = GetComponents<IEntityStateChangedListener>(metadata);
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}