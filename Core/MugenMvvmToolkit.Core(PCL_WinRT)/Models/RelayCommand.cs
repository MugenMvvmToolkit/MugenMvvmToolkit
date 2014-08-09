#region Copyright
// ****************************************************************************
// <copyright file="RelayCommand.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default
    ///     return value for the CanExecute method is 'true'.
    /// </summary>
    public class RelayCommand : RelayCommand<object>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        public RelayCommand([NotNull] Action<object> execute)
            : base(execute)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method, when a change
        ///     occurs.
        /// </param>
        public RelayCommand([NotNull] Action<object> execute, Predicate<object> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers)
        {
        }

        #endregion
    }
}