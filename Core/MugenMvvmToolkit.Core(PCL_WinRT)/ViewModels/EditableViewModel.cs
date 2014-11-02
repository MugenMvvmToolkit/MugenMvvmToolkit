#region Copyright
// ****************************************************************************
// <copyright file="EditableViewModel.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.ViewModels
{
    /// <summary>
    ///     Represent the class for editable view models.
    /// </summary>
    [BaseViewModel(Priority = 5)]
    public abstract class EditableViewModel<T> : ValidatableViewModel, IEditableViewModel<T> where T : class
    {
        #region Fields

        private IEntitySnapshot _entitySnapshot;
        private T _entity;
        private T _initializedEntity;

        private bool _hasChanges;
        private bool _isNewRecord;

        private EventHandler<IEditableViewModel, EntityInitializedEventArgs> _entityEntityInitializedNonGeneric;
        private EventHandler<IEditableViewModel, ChangesCanceledEventArgs> _changesCanceledNonGeneric;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditableViewModel{T}" /> class.
        /// </summary>
        static EditableViewModel()
        {
            UpdateChangesOnPropertyChangedDefault = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditableViewModel{T}" /> class.
        /// </summary>
        protected EditableViewModel()
        {
            Func<Type, object> entityFactory = ServiceProvider.DefaultEntityFactory;
            if (entityFactory != null)
                Entity = (T)entityFactory(typeof(T));
            UpdateChangesOnPropertyChanged = UpdateChangesOnPropertyChangedDefault;
        }

        #endregion

        #region Implementation of IEditableViewModel

        /// <summary>
        ///     Gets the type of model.
        /// </summary>
        public Type ModelType
        {
            get { return typeof(T); }
        }

        /// <summary>
        ///     Gets the value which indicates that is the new entity or not.
        /// </summary>
        public bool IsNewRecord
        {
            get { return _isNewRecord; }
            protected set
            {
                if (value.Equals(_isNewRecord)) return;
                _isNewRecord = value;
                OnPropertyChanged("IsNewRecord");
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the entity has changes.
        /// </summary>
        public virtual bool HasChanges
        {
            get
            {
                if (!IsEntityInitialized)
                    return false;
                if (_entitySnapshot != null && _entitySnapshot.SupportChangeDetection)
                    return _hasChanges || _entitySnapshot.HasChanges(Entity);
                return _hasChanges;
            }
            protected set
            {
                if (value.Equals(_hasChanges)) return;
                _hasChanges = value;
                OnPropertyChanged("HasChanges");
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the entity is initialized.
        /// </summary>
        public virtual bool IsEntityInitialized
        {
            get { return _initializedEntity != null; }
        }

        /// <summary>
        ///     Gets the edited entity.
        /// </summary>
        object IEditableViewModel.Entity
        {
            get { return Entity; }
        }

        /// <summary>
        ///     Initializes the specified entity to edit.
        /// </summary>
        /// <param name="entity">The specified entity to edit.</param>
        /// <param name="isNewRecord">
        ///     If <c>true</c> is new entity;otherwise <c>false</c>.
        /// </param>
        void IEditableViewModel.InitializeEntity(object entity, bool isNewRecord)
        {
            InitializeEntity((T)entity, isNewRecord);
        }

        /// <summary>
        ///     Applies the changes of entity.
        /// </summary>
        /// <returns>A series of instances of <see cref="IEntityStateEntry" />.</returns>
        public IList<IEntityStateEntry> ApplyChanges()
        {
            EnsureNotDisposed();
            if (!IsEntityInitialized)
                throw ExceptionManager.EditorNotInitialized(this);
            T entity;
            IList<IEntityStateEntry> result = ApplyChangesInternal(out entity) ??
                                              Empty.Array<IEntityStateEntry>();
            Entity = entity;
            Should.PropertyBeNotNull(Entity, "Entity");
            OnChangesApplied(result);
            RaiseChangesApplied(result);
            return result;
        }

        /// <summary>
        ///     Cancels the changes of entity.
        /// </summary>
        /// <returns>An instance of object.</returns>
        object IEditableViewModel.CancelChanges()
        {
            return CancelChanges();
        }

        /// <summary>
        ///     Gets the edited entity.
        /// </summary>
        public T Entity
        {
            get { return _entity; }
            protected set
            {
                if (ReferenceEquals(value, _entity))
                    return;
                _entity = value;
                OnPropertyChanged("Entity");
            }
        }

        /// <summary>
        ///     Initializes the specified entity to edit.
        /// </summary>
        /// <param name="entity">The specified entity to edit.</param>
        /// <param name="isNewRecord">
        ///     If <c>true</c> is new entity;otherwise <c>false</c>.
        /// </param>
        public void InitializeEntity(T entity, bool isNewRecord)
        {
            Should.NotBeNull(entity, "entity");
            EnsureNotDisposed();
            IsNewRecord = isNewRecord;
            var saved = SaveEntityState(entity);
            Entity = saved;
            OnBeginEdit();
            OnEntityInitialized();
            RaiseEntityInitialized(entity, saved);
        }

        /// <summary>
        ///     Cancels the changes of entity.
        /// </summary>
        /// <returns>An instance of object.</returns>
        public T CancelChanges()
        {
            EnsureNotDisposed();
            if (!IsEntityInitialized)
                throw ExceptionManager.EditorNotInitialized(this);
            T cancel = CancelChangesInternal();
            Entity = cancel;
            OnChangesCanceled();
            RaiseChangesCanceled(cancel);
            return cancel;
        }

        /// <summary>
        ///     Occurs at the end of initialization the entity.
        /// </summary>
        event EventHandler<IEditableViewModel, EntityInitializedEventArgs> IEditableViewModel.EntityInitialized
        {
            add { _entityEntityInitializedNonGeneric += value; }
            remove { _entityEntityInitializedNonGeneric -= value; }
        }

        /// <summary>
        ///     Occurs at the end of cancel entity changes.
        /// </summary>
        event EventHandler<IEditableViewModel, ChangesCanceledEventArgs> IEditableViewModel.ChangesCanceled
        {
            add { _changesCanceledNonGeneric += value; }
            remove { _changesCanceledNonGeneric -= value; }
        }

        /// <summary>
        ///     Occurs at the end of initialization the entity.
        /// </summary>
        public virtual event EventHandler<IEditableViewModel, EntityInitializedEventArgs<T>> EntityInitialized;

        /// <summary>
        ///     Occurs at the end of apply entity changes.
        /// </summary>
        public virtual event EventHandler<IEditableViewModel, ChangesAppliedEventArgs> ChangesApplied;

        /// <summary>
        ///     Occurs at the end of cancel entity changes.
        /// </summary>
        public virtual event EventHandler<IEditableViewModel, ChangesCanceledEventArgs<T>> ChangesCanceled;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether the editor should automatically update changes when an entity property is changed.
        /// </summary>
        public static bool UpdateChangesOnPropertyChangedDefault { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the editor should automatically update changes when an entity property is changed.
        /// </summary>
        public bool UpdateChangesOnPropertyChanged { get; set; }

        /// <summary>
        ///     Gets the entity state manager.
        /// </summary>
        public virtual IEntityStateManager StateManager { get; protected set; }

        /// <summary>
        /// Gets the entity state snapshot, if any.
        /// </summary>
        [CanBeNull]
        protected IEntitySnapshot EntitySnapshot
        {
            get { return _entitySnapshot; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds a property name to the <see cref="ValidatableViewModel.IgnoreProperties" />.
        /// </summary>
        protected void AddIgnoreProperty<TValue>(Expression<Func<T, TValue>> getProperty)
        {
            IgnoreProperties.Add(getProperty.GetMemberInfo().Name);
        }

        /// <summary>
        ///     Removes a property name to the <see cref="ValidatableViewModel.IgnoreProperties" />.
        /// </summary>
        protected void RemoveIgnoreProperty<TValue>(Expression<Func<T, TValue>> getProperty)
        {
            IgnoreProperties.Remove(getProperty.GetMemberInfo().Name);
        }

        /// <summary>
        ///     Occurs after an entity instance is initialized.
        /// </summary>
        protected virtual void OnEntityInitialized()
        {
        }

        /// <summary>
        ///     Saves the state of entity.
        /// </summary>
        /// <returns>An instance of object.</returns>
        protected virtual T SaveEntityState(T entity)
        {
            if (StateManager != null)
                _entitySnapshot = StateManager.CreateSnapshot(entity);
            return entity;
        }

        /// <summary>
        ///     Applies the changes of entity.
        /// </summary>
        /// <returns>A series of instances of <see cref="IEntityStateEntry" />.</returns>
        protected virtual IList<IEntityStateEntry> ApplyChangesInternal(out T entity)
        {
            entity = Entity;
            return GetChanges(entity);
        }

        /// <summary>
        ///     Gets the changes.
        /// </summary>
        /// <param name="entity">The saved entity.</param>
        /// <returns>A series of instances of <see cref="IEntityStateEntry" />.</returns>
        protected virtual IList<IEntityStateEntry> GetChanges(T entity)
        {
            return new List<IEntityStateEntry>
            {
                new EntityStateEntry(IsNewRecord ? EntityState.Added : EntityState.Modified, entity)
            };
        }

        /// <summary>
        ///     Occurs after applying changes.
        /// </summary>
        /// <param name="entityStateEntries">The entity state entries.</param>
        protected virtual void OnChangesApplied(IList<IEntityStateEntry> entityStateEntries)
        {
        }

        /// <summary>
        ///     Cancels the changes of entity.
        /// </summary>
        /// <returns>An instance of object.</returns>
        protected virtual T CancelChangesInternal()
        {
            if (_entitySnapshot != null)
                _entitySnapshot.Restore(Entity);
            return Entity;
        }

        /// <summary>
        ///     Occurs after canceling changes.
        /// </summary>
        protected virtual void OnChangesCanceled()
        {
        }

        /// <summary>
        ///     Raises the <c>EntityInitialized</c> event
        /// </summary>
        protected void RaiseEntityInitialized(T originalEntity, T entity)
        {
            EntityInitializedEventArgs<T> args = null;
            var genericHandler = EntityInitialized;
            if (genericHandler != null)
            {
                args = new EntityInitializedEventArgs<T>(originalEntity, entity);
                genericHandler(this, args);
            }
            var handler = _entityEntityInitializedNonGeneric;
            if (handler != null)
                handler(this, args ?? new EntityInitializedEventArgs<T>(originalEntity, entity));
        }

        /// <summary>
        ///     Raises the <c>ChangesApplied</c> event
        /// </summary>
        protected void RaiseChangesApplied(IList<IEntityStateEntry> entityStateEntries)
        {
            var handler = ChangesApplied;
            if (handler != null)
                handler(this, new ChangesAppliedEventArgs(entityStateEntries));
        }

        /// <summary>
        ///     Raises the <c>ChangesCanceled</c> event
        /// </summary>
        protected void RaiseChangesCanceled(T entity)
        {
            ChangesCanceledEventArgs<T> args = null;
            var genericHandler = ChangesCanceled;
            if (genericHandler != null)
            {
                args = new ChangesCanceledEventArgs<T>(entity);
                genericHandler(this, args);
            }
            var handler = _changesCanceledNonGeneric;
            if (handler != null)
                handler(this, args ?? new ChangesCanceledEventArgs<T>(entity));
        }

        /// <summary>
        ///     Adds a property mapping to the <see cref="ValidatableViewModel.PropertyMappings" /> dictionary.
        /// </summary>
        protected void AddPropertyMapping<T1, T2>([NotNull] Expression<Func<T1>> viewModelProperty,
            [NotNull] Expression<Func<T, T2>> modelProperty)
        {
            var vmProperty = viewModelProperty.GetMemberInfo().Name;
            var mProperty = modelProperty.GetMemberInfo().Name;
            ICollection<string> value;
            if (!PropertyMappings.TryGetValue(vmProperty, out value))
            {
                value = new HashSet<string>();
                PropertyMappings[vmProperty] = value;
            }
            value.Add(mProperty);
        }

        private void OnBeginEdit()
        {
            Should.PropertyBeNotNull(Entity, "Entity");
            if (_initializedEntity == null)
                AddInstance(this);
            else
            {
                RemoveInstance(_initializedEntity);
                ClearErrors();
            }
            _initializedEntity = Entity;
            AddInstance(_initializedEntity);
            ValidateAsync();
            HasChanges = IsNewRecord;
            OnPropertyChanged(string.Empty);
        }

        #endregion

        #region Overrides of ValidatableViewModel

        /// <summary>
        ///     Occurs after the initialization of the current <see cref="ViewModelBase" />.
        /// </summary>
        internal override void OnInitializedInternal()
        {
            if (StateManager == null)
                StateManager = IocContainer.Get<IEntityStateManager>();
            if (ValidatorProvider == null)
                ValidatorProvider = IocContainer.Get<IValidatorProvider>();
        }

        /// <summary>
        ///     Occurs after current view model disposed, use for clear resource and event listeners(Internal only).
        /// </summary>
        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                ChangesApplied = null;
                ChangesCanceled = null;
                EntityInitialized = null;
                _entityEntityInitializedNonGeneric = null;
                _changesCanceledNonGeneric = null;
            }
            base.OnDisposeInternal(disposing);
        }

        internal override void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
            base.OnPropertyChangedInternal(args);
            if (IsEntityInitialized && UpdateChangesOnPropertyChanged &&
                !IgnoreProperties.Contains(args.PropertyName) && !IsDisposed)
                OnPropertyChanged("HasChanges");
        }

        #endregion
    }
}