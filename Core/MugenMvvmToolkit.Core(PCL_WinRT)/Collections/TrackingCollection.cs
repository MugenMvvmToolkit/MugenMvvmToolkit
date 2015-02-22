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
    /// <summary>
    ///     Represents the collection that allows to track the changes of entities.
    /// </summary>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackingCollection" /> class.
        /// </summary>
        public TrackingCollection(IEqualityComparer<object> comparer = null)
            : this(null, comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackingCollection" /> class.
        /// </summary>
        public TrackingCollection(IStateTransitionManager stateTransitionManager, IEqualityComparer<object> comparer = null)
        {
            if (stateTransitionManager == null)
                stateTransitionManager = Infrastructure.StateTransitionManager.Instance;
            StateTransitionManagerInternal = stateTransitionManager;
            ItemsInternal = new Dictionary<object, EntityState>(comparer ?? ReferenceEqualityComparer.Instance);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackingCollection" /> class.
        /// </summary>
        public TrackingCollection([NotNull] IEnumerable<object> collection,
            EntityState entityState = EntityState.Unchanged,
            IStateTransitionManager stateTransitionManager = null, IEqualityComparer<object> comparer = null)
            : this(stateTransitionManager, comparer)
        {
            Should.NotBeNull(collection, "collection");
            foreach (object item in collection)
                ItemsInternal.Add(item, entityState);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackingCollection" /> class.
        /// </summary>
        public TrackingCollection([NotNull] IEnumerable<IEntityStateEntry> changes,
            IStateTransitionManager stateTransitionManager = null, IEqualityComparer<object> comparer = null)
            : this(stateTransitionManager, comparer)
        {
            Should.NotBeNull(changes, "changes");
            foreach (var item in changes)
                ItemsInternal.Add(item.Entity, item.State);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackingCollection" /> class.
        /// </summary>
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

        /// <summary>
        ///     Gets the internal dictionary with entity and their states.
        /// </summary>
        protected Dictionary<object, EntityState> Items
        {
            get { return ItemsInternal; }
        }

        /// <summary>
        ///     Gets an object that can be used to synchronize access
        /// </summary>
        protected object Locker
        {
            get { return ItemsInternal; }
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
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

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ITrackingCollection

        /// <summary>
        ///     Gets or sets the property that indicates that the state will be validated before assigned.
        /// </summary>
        public bool ValidateState { get; set; }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="ITrackingCollection" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="ITrackingCollection" />.
        /// </returns>
        public int Count
        {
            get { return ItemsInternal.Count; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IStateTransitionManager" />.
        /// </summary>
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

        /// <summary>
        ///     Gets a value indicating whether the collection has changes, including new, deleted, or modified values.
        /// </summary>
        public bool HasChanges
        {
            get { return ChangedCount != 0; }
        }

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Determines whether the <see cref="ITrackingCollection" /> contains a specific value.
        /// </summary>
        public bool Contains(object item)
        {
            Should.NotBeNull(item, "item");
            lock (Locker)
                return ItemsInternal.ContainsKey(item);
        }

        /// <summary>
        ///     Determines whether the <see cref="ITrackingCollection" /> contains a specific value.
        /// </summary>
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

        /// <summary>
        ///     Gets an array of all objects with specified entity state.
        /// </summary>
        /// <returns>
        ///     An array of objects.
        /// </returns>
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

        /// <summary>
        ///     Gets the changes of objects.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="IList{T}" />.
        /// </returns>
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

        /// <summary>
        ///     Gets the object state.
        /// </summary>
        /// <param name="value">The specified value.</param>
        /// <returns>
        ///     An instance of <see cref="EntityState" />.
        /// </returns>
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

        /// <summary>
        ///     Updates a state in the specified value.
        /// </summary>
        /// <param name="value">The specified value to update state.</param>
        /// <param name="state">The state value.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        /// <returns>
        ///     If <c>true</c> the state was changed; otherwise <c>false</c>.
        /// </returns>
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

        /// <summary>
        ///     Removes all items from the <see cref="ITrackingCollection" />.
        /// </summary>
        public void Clear()
        {
            lock (Locker)
                ClearInternal();
            OnPropertyChanged(Empty.HasChangesChangedArgs);
            OnPropertyChanged(Empty.CountChangedArgs);
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        public virtual ITrackingCollection Clone()
        {
            lock (Locker)
                return CloneInternal();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates the state of value.
        /// </summary>
        /// <param name="value">The specified value.</param>
        /// <param name="state">The specified state.</param>
        /// <param name="validate">The flag indicating that state will be validated before assigned.</param>
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

        /// <summary>
        ///     Removes all items from the <see cref="ITrackingCollection" />.
        /// </summary>
        protected virtual void ClearInternal()
        {
            ItemsInternal.Clear();
            ChangedCount = 0;
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, args);
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
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