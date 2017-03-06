#region Copyright

// ****************************************************************************
// <copyright file="EditableViewModelMock.cs">
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

        protected override void OnEntityInitialized()
        {
            IsEntityInitializedInvoked = true;
        }

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

        protected override IList<IEntityStateEntry> GetChanges(object entity)
        {
            if (GetChangesDelegate != null)
                return GetChangesDelegate(entity);
            return base.GetChanges(entity);
        }

        protected override void OnChangesApplied(IList<IEntityStateEntry> entityStateEntries)
        {
            if (OnChangesAppliedDelegate != null)
                OnChangesAppliedDelegate(entityStateEntries);
        }

        protected override object CancelChangesInternal()
        {
            if (CancelChangesInternalDelegate != null)
                return CancelChangesInternalDelegate();
            return base.CancelChangesInternal();
        }

        protected override void OnChangesCanceled()
        {
            IsChangesCanceledInvoked = true;
        }

        #endregion
    }
}
