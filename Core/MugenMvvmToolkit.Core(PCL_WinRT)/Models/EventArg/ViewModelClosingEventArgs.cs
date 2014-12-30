#region Copyright

// ****************************************************************************
// <copyright file="ViewModelClosingEventArgs.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewModelClosingEventArgs : ViewModelClosedEventArgs
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelClosingEventArgs" /> class.
        /// </summary>
        public ViewModelClosingEventArgs([NotNull]IViewModel viewModel, [CanBeNull]object parameter)
            : base(viewModel, parameter)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        /// <returns>
        ///     true if the event should be canceled; otherwise, false.
        /// </returns>
        public bool Cancel { get; set; }

        #endregion
    }
}