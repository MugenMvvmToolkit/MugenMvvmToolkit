#region Copyright

// ****************************************************************************
// <copyright file="EditableViewModel.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

        protected EditableViewModel()
        {
            Func<Type, object> entityFactory = ServiceProvider.DefaultEntityFactory;
            if (entityFactory != null)
                Entity = (T)entityFactory(typeof(T));
        }

        #endregion

        #region Implementation of IEditableViewModel

        public Type ModelType => typeof(T);

        public bool IsNewRecord
        {
            get { return _isNewRecord; }
            protected set
            {
                if (value.Equals(_isNewRecord)) return;
                _isNewRecord = value;
                OnPropertyChanged();
            }
        }

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
                OnPropertyChanged(Empty.HasChangesChangedArgs);
            }
        }

        public virtual bool IsEntityInitialized => _initializedEntity != null;

        object IEditableViewModel.Entity => Entity;

        void IEditableViewModel.InitializeEntity(object entity, bool isNewRecord)
        {
            InitializeEntity((T)entity, isNewRecord);
        }

        public IList<IEntityStateEntry> ApplyChanges()
        {
            EnsureNotDisposed();
            if (!IsEntityInitialized)
                throw ExceptionManager.EditorNotInitialized(this);
            T entity;
            IList<IEntityStateEntry> result = ApplyChangesInternal(out entity) ??
                                              Empty.Array<IEntityStateEntry>();
            Entity = entity;
            Should.PropertyNotBeNull(Entity, nameof(Entity));
            OnChangesApplied(result);
            RaiseChangesApplied(result);
            InvalidateProperties();
            return result;
        }

        object IEditableViewModel.CancelChanges()
        {
            return CancelChanges();
        }

        public T Entity
        {
            get { return _entity; }
            protected set
            {
                if (ReferenceEquals(value, _entity))
                    return;
                _entity = value;
                OnPropertyChanged();
            }
        }

        public void InitializeEntity(T entity, bool isNewRecord)
        {
            Should.NotBeNull(entity, nameof(entity));
            EnsureNotDisposed();
            IsNewRecord = isNewRecord;
            var saved = SaveEntityState(entity);
            Entity = saved;
            OnBeginEdit();
            OnEntityInitialized();
            RaiseEntityInitialized(entity, saved);
        }

        public T CancelChanges()
        {
            EnsureNotDisposed();
            if (!IsEntityInitialized)
                throw ExceptionManager.EditorNotInitialized(this);
            T cancel = CancelChangesInternal();
            Entity = cancel;
            OnChangesCanceled();
            RaiseChangesCanceled(cancel);
            InvalidateProperties();
            return cancel;
        }

        event EventHandler<IEditableViewModel, EntityInitializedEventArgs> IEditableViewModel.EntityInitialized
        {
            add { _entityEntityInitializedNonGeneric += value; }
            remove { _entityEntityInitializedNonGeneric -= value; }
        }

        event EventHandler<IEditableViewModel, ChangesCanceledEventArgs> IEditableViewModel.ChangesCanceled
        {
            add { _changesCanceledNonGeneric += value; }
            remove { _changesCanceledNonGeneric -= value; }
        }

        public virtual event EventHandler<IEditableViewModel, EntityInitializedEventArgs<T>> EntityInitialized;

        public virtual event EventHandler<IEditableViewModel, ChangesAppliedEventArgs> ChangesApplied;

        public virtual event EventHandler<IEditableViewModel, ChangesCanceledEventArgs<T>> ChangesCanceled;

        #endregion

        #region Properties

        public virtual IEntityStateManager StateManager { get; protected set; }

        [CanBeNull]
        protected IEntitySnapshot EntitySnapshot => _entitySnapshot;

        #endregion

        #region Methods

        protected void AddIgnoreProperty(Func<Expression<Func<T, object>>> getProperty)
        {
            IgnoreProperties.Add(getProperty.GetMemberName());
        }

        protected void RemoveIgnoreProperty(Func<Expression<Func<T, object>>> getProperty)
        {
            IgnoreProperties.Remove(getProperty.GetMemberName());
        }

        protected virtual void OnEntityInitialized()
        {
        }

        protected virtual T SaveEntityState(T entity)
        {
            if (StateManager != null)
                _entitySnapshot = StateManager.CreateSnapshot(entity, Settings.Metadata);
            return entity;
        }

        protected virtual IList<IEntityStateEntry> ApplyChangesInternal(out T entity)
        {
            entity = Entity;
            return GetChanges(entity);
        }

        protected virtual IList<IEntityStateEntry> GetChanges(T entity)
        {
            var changes = new List<IEntityStateEntry>();
            if (IsNewRecord)
                changes.Add(new EntityStateEntry(EntityState.Added, entity));
            else if (HasChanges)
                changes.Add(new EntityStateEntry(EntityState.Modified, entity));
            return changes;
        }

        protected virtual void OnChangesApplied(IList<IEntityStateEntry> entityStateEntries)
        {
        }

        protected virtual T CancelChangesInternal()
        {
            _entitySnapshot?.Restore(Entity);
            return Entity;
        }

        protected virtual void OnChangesCanceled()
        {
        }

        protected virtual void RaiseEntityInitialized(T originalEntity, T entity)
        {
            EntityInitializedEventArgs<T> args = null;
            var genericHandler = EntityInitialized;
            if (genericHandler != null)
            {
                args = new EntityInitializedEventArgs<T>(originalEntity, entity);
                genericHandler(this, args);
            }
            _entityEntityInitializedNonGeneric?.Invoke(this, args ?? new EntityInitializedEventArgs<T>(originalEntity, entity));
        }

        protected virtual void RaiseChangesApplied(IList<IEntityStateEntry> entityStateEntries)
        {
            ChangesApplied?.Invoke(this, new ChangesAppliedEventArgs(entityStateEntries));
        }

        protected virtual void RaiseChangesCanceled(T entity)
        {
            ChangesCanceledEventArgs<T> args = null;
            var genericHandler = ChangesCanceled;
            if (genericHandler != null)
            {
                args = new ChangesCanceledEventArgs<T>(entity);
                genericHandler(this, args);
            }
            _changesCanceledNonGeneric?.Invoke(this, args ?? new ChangesCanceledEventArgs<T>(entity));
        }

        protected void AddPropertyMapping<TViewModel>([NotNull] Func<Expression<Func<TViewModel, object>>> viewModelProperty,
            [NotNull] Func<Expression<Func<T, object>>> modelProperty)
        {
            var vmProperty = viewModelProperty.GetMemberName();
            var mProperty = modelProperty.GetMemberName();
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
            Should.PropertyNotBeNull(Entity, nameof(Entity));
            object oldInstance = _initializedEntity;
            if (oldInstance != null)
            {
                RemoveInstance(oldInstance);
                ClearErrors();
            }
            _initializedEntity = Entity;
            AddInstance(_initializedEntity);
            if (oldInstance == null)
                AddInstance(this);
            ValidateAsync();
            HasChanges = IsNewRecord;
            InvalidateProperties();
        }

        #endregion

        #region Overrides of ValidatableViewModel

        internal override void OnInitializedInternal()
        {
            if (StateManager == null)
                StateManager = IocContainer.Get<IEntityStateManager>();
            if (ValidatorProvider == null)
                ValidatorProvider = IocContainer.Get<IValidatorProvider>();
        }

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
            if (IsEntityInitialized && !IgnoreProperties.Contains(args.PropertyName) && !IsDisposed)
                OnPropertyChanged(Empty.HasChangesChangedArgs, ExecutionMode.None);
        }

        #endregion
    }
}
