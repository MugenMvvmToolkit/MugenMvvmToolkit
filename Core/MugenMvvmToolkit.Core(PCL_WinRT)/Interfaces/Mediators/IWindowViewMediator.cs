#region Copyright
// ****************************************************************************
// <copyright file="IWindowViewMediator.cs">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Interfaces.Mediators
{
    /// <summary>
    ///     Represents the mediator interface for the dialog view.
    /// </summary>
    public interface IWindowViewMediator
    {
        /// <summary>
        ///     Gets a value that indicates whether the dialog is visible. true if the dialog is visible; otherwise, false.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        ///     Gets the <see cref="IView" />.
        /// </summary>
        [CanBeNull]
        IView View { get; }

        /// <summary>
        ///     Gets the underlying view model.
        /// </summary>
        [NotNull]
        IViewModel ViewModel { get; }

        /// <summary>
        ///     Shows the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="callback">The specified callback, if any.</param>
        /// <param name="context">The specified context.</param>
        void Show([CanBeNull] IOperationCallback callback, [CanBeNull] IDataContext context);

        /// <summary>
        ///     Tries to close view-model.
        /// </summary>
        /// <param name="parameter">The specified parameter, if any.</param>
        /// <returns>An instance of task with result.</returns>
        [NotNull]
        Task<bool> CloseAsync([CanBeNull] object parameter);

        /// <summary>
        ///     Updates the current view, for example android platform use this API to update view after recreate a dialog
        ///     fragment.
        /// </summary>
        void UpdateView([CanBeNull] IView view, bool isOpen, [CanBeNull] IDataContext context);
    }
}