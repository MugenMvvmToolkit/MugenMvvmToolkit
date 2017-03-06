#region Copyright

// ****************************************************************************
// <copyright file="TrackingCollection.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Collections
{
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable, KnownType(typeof(CompositeEqualityComparer))]
    public class TrackingCollection : ITrackingCollection, IStateTransitionManager
    {
        #region Fields

        [DataMember]
        internal readonly Dictionary<object, EntityState> ItemsInternal;

        [DataMember]
        internal IStateTransitionManager StateTransitionManagerInternal;

        [DataMember]
        internal long ChangedCount;

        #endregion

        #region Constructors

        public TrackingCollection()
            : this(null)
        {
        }

        public TrackingCollection(IEqualityComparer<object> comparer)
            : this(null, comparer)
        {
        }

        public TrackingCollection(IStateTransitionManager stateTransitionManager, IEqualityComparer<object> comparer = null)
        {
            StateTransitionManager = stateTransitionManager;
            ItemsInternal = new Dictionary<object, EntityState>(comparer ?? ReferenceEqualityComparer.Instance);
        }

        public TrackingCollection([NotNull] IEnumerable<object> collection,
            EntityState entityState = EntityState.Unchanged,
            IStateTransitionManager stateTransitionManager = null, IEqualityComparer<object> comparer = null)
            : this(stateTransitionManager, comparer)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (object item in collection)
                ItemsInternal.Add(item, entityState);
        }

        public TrackingCollection([NotNull] IEnumerable<IEntityStateEntry> changes,
            IStateTransitionManager stateTransitionManager = null, IEqualityComparer<object> comparer = null)
            : this(stateTransitionManager, comparer)
        {
            Should.NotBeNull(changes, nameof(changes));
            foreach (var item in changes)
                ItemsInternal.Add(item.Entity, item.State);
        }

        public TrackingCollection([NotNull] IEnumerable<KeyValuePair<object, EntityState>> changes,
            IStateTransitionManager stateTransitionManager = null, IEqualityComparer<object> comparer = null)
            : this(stateTransitionManager, comparer)
        {
            Should.NotBeNull(changes, nameof(changes));
            foreach (var item in changes)
                ItemsInternal.Add(item.Key, item.Value);
        }

        #endregion

        #region Properties

        protected Dictionary<object, EntityState> Items => ItemsInternal;

        protected object Locker => ItemsInternal;

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<TrackingEntity<object>> GetEnumerator()
        {
            var list = new List<TrackingEntity<object>>(Count);
            lock (Locker)
            {
                foreach (var keyValuePair in ItemsInternal)
                    list.Add(new TrackingEntity<object>(keyValuePair.Key, keyValuePair.Value));
            }
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ITrackingCollection

        public int Count => ItemsInternal.Count;

        [IgnoreDataMember]
        public IStateTransitionManager StateTransitionManager
        {
            get
            {
                if (StateTransitionManagerInternal == null)
                    return this;
                return StateTransitionManagerInternal;
            }
            set
            {
                if (value != this)
                    StateTransitionManagerInternal = value;
                OnPropertyChanged(new PropertyChangedEventArgs("StateTransitionManager"));
            }
        }

        public bool HasChanges => ChangedCount != 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Contains(object item)
        {
            Should.NotBeNull(item, nameof(item));
            lock (Locker)
                return ItemsInternal.ContainsKey(item);
        }

        public bool Contains<TEntity>(Func<TrackingEntity<TEntity>, bool> predicate)
        {
            Should.NotBeNull(predicate, nameof(predicate));
            lock (Locker)
            {
                foreach (var keyValuePair in ItemsInternal)
                {
                    if (!(keyValuePair.Key is TEntity)) continue;
                    var item = (TEntity)keyValuePair.Key;
                    if (!predicate(new TrackingEntity<TEntity>(item, keyValuePair.Value)))
                        continue;
                    return true;
                }
                return false;
            }
        }

        public IList<TEntity> Find<TEntity>(Func<TrackingEntity<TEntity>, bool> predicate)
        {
            var result = new List<TEntity>();
            lock (Locker)
            {
                foreach (var keyValuePair in ItemsInternal)
                {
                    if (!(keyValuePair.Key is TEntity)) continue;
                    var item = (TEntity)keyValuePair.Key;
                    if (predicate == null || predicate(new TrackingEntity<TEntity>(item, keyValuePair.Value)))
                        result.Add(item);
                }
            }
            return result;
        }

        public IList<IEntityStateEntry> GetChanges(EntityState entityState = EntityState.Added | EntityState.Modified | EntityState.Deleted)
        {
            var result = new List<IEntityStateEntry>();
            lock (Locker)
            {
                foreach (var pair in ItemsInternal)
                {
                    if (entityState.HasState(pair.Value))
                        result.Add(new EntityStateEntry(pair.Value, pair.Key));
                }
            }
            return result;
        }

        public EntityState GetState(object value)
        {
            Should.NotBeNull(value, nameof(value));
            lock (Locker)
            {
                EntityState result;
                if (ItemsInternal.TryGetValue(value, out result))
                    return result;
                return EntityState.Detached;
            }
        }

        public bool UpdateState(object value, EntityState state)
        {
            Should.NotBeNull(value, nameof(value));
            bool updated;
            lock (Locker)
                updated = UpdateStateInternal(value, state);
            if (updated)
            {
                OnPropertyChanged(Empty.HasChangesChangedArgs);
                OnPropertyChanged(Empty.CountChangedArgs);
            }
            return updated;
        }

        public void Clear()
        {
            lock (Locker)
                ClearInternal();
            OnPropertyChanged(Empty.HasChangesChangedArgs);
            OnPropertyChanged(Empty.CountChangedArgs);
        }

        public virtual ITrackingCollection Clone()
        {
            lock (Locker)
                return CloneInternal();
        }

        #endregion

        #region Implementation of IStateTransitionManager

        EntityState IStateTransitionManager.ChangeState(object item, EntityState @from, EntityState to)
        {
            switch (from)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Detached:
                    return to;
                case EntityState.Added:
                    switch (to)
                    {
                        case EntityState.Deleted:
                            return EntityState.Detached;
                        case EntityState.Modified:
                            return EntityState.Added;
                        default:
                            return to;
                    }
                case EntityState.Deleted:
                    switch (to)
                    {
                        case EntityState.Added:
                            return EntityState.Modified;
                        default:
                            return to;
                    }
                default:
                    throw ExceptionManager.EnumOutOfRange("from", from);
            }
        }

        #endregion

        #region Methods

        protected virtual bool UpdateStateInternal(object value, EntityState state)
        {
            if (value == null)
                return false;
            EntityState entityState;
            if (!ItemsInternal.TryGetValue(value, out entityState))
            {
                state = StateTransitionManager.ChangeState(value, EntityState.Detached, state);
                if (!state.IsDetached())
                {
                    ItemsInternal[value] = state;
                    if (state.IsAddedOrModifiedOrDeleted())
                        ChangedCount++;
                    return true;
                }
                return false;
            }
            state = StateTransitionManager.ChangeState(value, entityState, state);
            //To update key value
            ItemsInternal.Remove(value);
            if (entityState.IsAddedOrModifiedOrDeleted())
                ChangedCount--;

            if (!state.IsDetached())
            {
                ItemsInternal[value] = state;
                if (state.IsAddedOrModifiedOrDeleted())
                    ChangedCount++;
            }
            return true;
        }

        protected virtual void ClearInternal()
        {
            ItemsInternal.Clear();
            ChangedCount = 0;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        protected virtual ITrackingCollection CloneInternal()
        {
            IEnumerable<KeyValuePair<object, EntityState>> items;
            lock (Locker)
                items = ItemsInternal.ToArrayEx();
            return new TrackingCollection(items, StateTransitionManager, ItemsInternal.Comparer);
        }

        #endregion        
    }
}
