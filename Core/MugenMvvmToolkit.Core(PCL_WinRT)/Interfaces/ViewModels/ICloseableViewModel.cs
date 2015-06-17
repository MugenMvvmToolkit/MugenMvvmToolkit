#region Copyright

// ****************************************************************************
// <copyright file="ICloseableViewModel.cs">
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

using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    /// <summary>
    ///     Represents the interface that allows to close a view model.
    /// </summary>
    public interface ICloseableViewModel : IViewModel
    {
        /// <summary>
        ///     Gets or sets a command that attempts to remove this workspace from the user interface.
        /// </summary>
        ICommand CloseCommand { get; set; }

        /// <summary>
        ///     Tries to close view-model.
        /// </summary>
        /// <param name="parameter">The specified parameter, if any.</param>
        /// <returns>An instance of task with result.</returns>
        [NotNull]
        Task<bool> CloseAsync(object parameter = null);

        /// <summary>
        ///     Occurs when <see cref="ICloseableViewModel" /> is closing.
        /// </summary>
        event EventHandler<ICloseableViewModel, ViewModelClosingEventArgs> Closing;

        /// <summary>
        ///     Occurs when <see cref="ICloseableViewModel" /> is closed.
        /// </summary>
        event EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> Closed;
    }
}