#region Copyright

// ****************************************************************************
// <copyright file="TrackingCollection.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable, KnownType(typeof(StateTransitionManager)), KnownType(typeof(CompositeEqualityComparer))]
    public class TrackingCollection : ITrackingCollection
    {
        #region Fields

        private static readonly PropertyChangedEventArgs StateTransitionManagerArgs;

        [DataMember]
        internal readonly Dictionary<object, EntityState> ItemsInternal;

        [DataMember]
        internal IStateTransitionManager StateTransitionManagerInternal;

        [DataMember]
        internal long ChangedCount;

        #endregion

        #region Constructors

        static TrackingCollection()
        {
            StateTransitionManagerArgs = new PropertyChangedEventArgs("StateTransitionManager");
        }

        public TrackingCollection(IEqualityComparer<object> comparer = null)
            : this(null, comparer)
        {
        }

        public TrackingCollection(IStateTransitionManager stateTransitionManager, IEqualityComparer<object> comparer = null)
        {
            if (stateTransitionManager == null)
                stateTransitionManager = Infrastructure.StateTransitionManager.Instance;
            StateTransitionManagerInternal = stateTransitionManager;
            ItemsInternal = new Dictionary<object, EntityState>(comparer ?? ReferenceEqualityComparer.Instance);
        }

        public TrackingCollection([NotNull] IEnumerable<object> collection,
            EntityState entityState = EntityState.Unchanged,
            IStateTransitionManager stateTransitionManager = null, IEqualityComparer<object> comparer = null)
            : this(stateTransitionManager, comparer)
        {
            Should.NotBeNull(collection, "collection");
            foreach (object item in collection)
                ItemsInternal.Add(item, entityState);
        }

        public TrackingCollection([NotNull] IEnumerable<IEntityStateEntry> changes,
            IStateTransitionManager stateTransitionManager = null, IEqualityComparer<object> comparer = null)
            : this(stateTransitionManager, comparer)
        {
            Should.NotBeNull(changes, "changes");
            foreach (var item in changes)
                ItemsInternal.Add(item.Entity, item.State);
        }

        public TrackingCollection([NotNull] IEnumerable<KeyValuePair<object, EntityState>> changes,
            IStateTransitionManager stateTransitionManager = null, IEqualityComparer<object> comparer = null)
            : this(stateTransitionManager, comparer)
        {
            Should.NotBeNull(changes, "changes");
            foreach (var item in changes)
                ItemsInternal.Add(item.Key, item.Value);
        }

        #endregion

        #region Properties

        protected Dictionary<object, EntityState> Items
        {
            get { return ItemsInternal; }
        }

        protected object Locker
        {
            get { return ItemsInternal; }
        }

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

        public bool ValidateState { get; set; }

        public int Count
        {
            get { return ItemsInternal.Count; }
        }

        public IStateTransitionManager StateTransitionManager
        {
            get { return StateTransitionManagerInternal; }
            set
            {
                Should.PropertyNotBeNull(value);
                StateTransitionManagerInternal = value;
                OnPropertyChanged(StateTransitionManagerArgs);
            }
        }

        public bool HasChanges
        {
            get { return ChangedCount != 0; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Contains(object item)
        {
            Should.NotBeNull(item, "item");
            lock (Locker)
                return ItemsInternal.ContainsKey(item);
        }

        public bool Contains<TEntity>(Func<TrackingEntity<TEntity>, bool> predicate)
        {
            Should.NotBeNull(predicate, "predicate");
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
            Should.NotBeNull(value, "value");
            lock (Locker)
            {
                EntityState result;
                if (ItemsInternal.TryGetValue(value, out result))
                    return result;
                return EntityState.Detached;
            }
        }

        public bool UpdateState(object value, EntityState state, bool? validateState = null)
        {
            Should.NotBeNull(value, "value");
            bool updated;
            lock (Locker)
                updated = UpdateStateInternal(value, state, validateState.GetValueOrDefault(ValidateState));
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

        #region Methods

        protected virtual bool UpdateStateInternal(object value, EntityState state, bool validate)
        {
            if (value == null)
                return false;
            EntityState entityState;
            if (!ItemsInternal.TryGetValue(value, out entityState))
            {
                state = StateTransitionManager.ChangeState(EntityState.Detached, state, validate);
                if (!state.IsDetached())
                {
                    ItemsInternal[value] = state;
                    if (state.IsAddedOrModifiedOrDeleted())
                        ChangedCount++;
                    return true;
                }
                return false;
            }
            state = StateTransitionManager.ChangeState(entityState, state, validate);
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
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, args);
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
