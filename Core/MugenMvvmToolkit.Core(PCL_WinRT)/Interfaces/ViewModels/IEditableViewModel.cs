#region Copyright
// ****************************************************************************
// <copyright file="IEditableViewModel.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    /// <summary>
    ///     Represent the interface for editable view model.
    /// </summary>
    public interface IEditableViewModel : IValidatableViewModel
    {
        /// <summary>
        ///     Gets the type of model.
        /// </summary>
        [NotNull]
        Type ModelType { get; }

        /// <summary>
        ///     Gets the value which indicates that is the new entity or not.
        /// </summary>
        bool IsNewRecord { get; }

        /// <summary>
        ///     Gets a value indicating whether the entity is initialized.
        /// </summary>
        bool IsEntityInitialized { get; }

        /// <summary>
        ///     Gets a value indicating whether the entity has changes.
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        ///     Gets the edited entity.
        /// </summary>
        object Entity { get; }

        /// <summary>
        ///     Initializes the specified entity to edit.
        /// </summary>
        /// <param name="entity">The specified entity to edit.</param>
        /// <param name="isNewRecord">
        ///     If <c>true</c> is new entity;otherwise <c>false</c>.
        /// </param>
        void InitializeEntity([NotNull] object entity, bool isNewRecord);

        /// <summary>
        ///     Applies the changes of entity.
        /// </summary>
        /// <returns>A series of instances of <see cref="IEntityStateEntry" />.</returns>
        [NotNull]
        IList<IEntityStateEntry> ApplyChanges();

        /// <summary>
        ///     Cancels the changes of entity.
        /// </summary>
        /// <returns>An instance of object.</returns>
        [NotNull]
        object CancelChanges();

        /// <summary>
        ///     Occurs at the end of initialization the entity.
        /// </summary>
        event EventHandler<IEditableViewModel, EntityInitializedEventArgs> EntityInitialized;

        /// <summary>
        ///     Occurs at the end of apply entity changes.
        /// </summary>
        event EventHandler<IEditableViewModel, ChangesAppliedEventArgs> ChangesApplied;

        /// <summary>
        ///     Occurs at the end of cancel entity changes.
        /// </summary>
        event EventHandler<IEditableViewModel, ChangesCanceledEventArgs> ChangesCanceled;
    }

    /// <summary>
    ///     Represent the interface for editable view model.
    /// </summary>
    public interface IEditableViewModel<T> : IEditableViewModel where T : class
    {
        /// <summary>
        ///     Gets the edited entity.
        /// </summary>
        new T Entity { get; }

        /// <summary>
        ///     Initializes the specified entity to edit.
        /// </summary>
        /// <param name="entity">The specified entity to edit.</param>
        /// <param name="isNewRecord">
        ///     If <c>true</c> is new entity;otherwise <c>false</c>.
        /// </param>
        void InitializeEntity([NotNull] T entity, bool isNewRecord);

        /// <summary>
        ///     Cancels the changes of entity.
        /// </summary>
        /// <returns>An instance of object.</returns>
        [NotNull]
        new T CancelChanges();

        /// <summary>
        ///     Occurs at the end of initialization the entity.
        /// </summary>
        new event EventHandler<IEditableViewModel, EntityInitializedEventArgs<T>> EntityInitialized;

        /// <summary>
        ///     Occurs at the end of cancel entity changes.
        /// </summary>
        new event EventHandler<IEditableViewModel, ChangesCanceledEventArgs<T>> ChangesCanceled;
    }
}