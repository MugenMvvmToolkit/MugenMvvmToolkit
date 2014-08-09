using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestViewModels
{
    public class EditableViewModelMock : EditableViewModel<object>
    {
        #region Properties

        public IEntityStateManager CustomStateManager { get; set; }

        public Func<ApplyChangesClass> ApplyChangesInternalDelegate { get; set; }

        public Func<object, IList<IEntityStateEntry>> GetChangesDelegate { get; set; }

        public Func<object> CancelChangesInternalDelegate { get; set; }

        public Action<IList<IEntityStateEntry>> OnChangesAppliedDelegate { get; set; }

        public bool IsEntityInitializedInvoked { get; set; }

        public bool IsChangesCanceledInvoked { get; set; }

        #endregion

        #region Overrides of EditableViewModel<object>

        /// <summary>
        ///     Gets the entity state manager.
        /// </summary>
        public override IEntityStateManager StateManager
        {
            get
            {
                if (CustomStateManager != null)
                    return CustomStateManager;
                return base.StateManager;
            }
            protected set { base.StateManager = value; }
        }

        /// <summary>
        ///     Occurs after an entity instance is initialized.
        /// </summary>
        protected override void OnEntityInitialized()
        {
            IsEntityInitializedInvoked = true;
        }

        /// <summary>
        ///     Applies the changes of entity.
        /// </summary>
        /// <returns>A series of instances of <see cref="IEntityStateEntry" />.</returns>
        protected override IList<IEntityStateEntry> ApplyChangesInternal(out object entity)
        {
            if (ApplyChangesInternalDelegate != null)
            {
                ApplyChangesClass applyChangesInternalDelegate = ApplyChangesInternalDelegate();
                entity = applyChangesInternalDelegate.Entity;
                return applyChangesInternalDelegate.EntityStateEntries;
            }
            return base.ApplyChangesInternal(out entity);
        }

        /// <summary>
        ///     Gets the changes.
        /// </summary>
        /// <param name="entity">The saved entity.</param>
        /// <returns>A series of instances of <see cref="IEntityStateEntry" />.</returns>
        protected override IList<IEntityStateEntry> GetChanges(object entity)
        {
            if (GetChangesDelegate != null)
                return GetChangesDelegate(entity);
            return base.GetChanges(entity);
        }

        /// <summary>
        ///     Occurs after applying changes.
        /// </summary>
        /// <param name="entityStateEntries">The entity state entries.</param>
        protected override void OnChangesApplied(IList<IEntityStateEntry> entityStateEntries)
        {
            if (OnChangesAppliedDelegate != null)
                OnChangesAppliedDelegate(entityStateEntries);
        }

        /// <summary>
        ///     Cancels the changes of entity.
        /// </summary>
        /// <returns>An instance of object.</returns>
        protected override object CancelChangesInternal()
        {
            if (CancelChangesInternalDelegate != null)
                return CancelChangesInternalDelegate();
            return base.CancelChangesInternal();
        }

        /// <summary>
        ///     Occurs after canceling changes.
        /// </summary>
        protected override void OnChangesCanceled()
        {
            IsChangesCanceledInvoked = true;
        }

        #endregion
    }
}