#region Copyright

// ****************************************************************************
// <copyright file="IEditableViewModel.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface IEditableViewModel : IValidatableViewModel
    {
        [NotNull]
        Type ModelType { get; }

        bool IsNewRecord { get; }

        bool IsEntityInitialized { get; }

        bool HasChanges { get; }

        object Entity { get; }

        void InitializeEntity([NotNull] object entity, bool isNewRecord);

        [NotNull]
        IList<IEntityStateEntry> ApplyChanges();

        [NotNull]
        object CancelChanges();

        event EventHandler<IEditableViewModel, EntityInitializedEventArgs> EntityInitialized;

        event EventHandler<IEditableViewModel, ChangesAppliedEventArgs> ChangesApplied;

        event EventHandler<IEditableViewModel, ChangesCanceledEventArgs> ChangesCanceled;
    }

    public interface IEditableViewModel<T> : IEditableViewModel where T : class
    {
        new T Entity { get; }

        void InitializeEntity([NotNull] T entity, bool isNewRecord);

        [NotNull]
        new T CancelChanges();

        new event EventHandler<IEditableViewModel, EntityInitializedEventArgs<T>> EntityInitialized;

        new event EventHandler<IEditableViewModel, ChangesCanceledEventArgs<T>> ChangesCanceled;
    }
}
