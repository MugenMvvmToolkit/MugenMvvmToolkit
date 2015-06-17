#region Copyright

// ****************************************************************************
// <copyright file="IWindowView.cs">
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

using System;
using System.ComponentModel;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WinRT.Interfaces.Views
{
    /// <summary>
    ///     Represent the base interface for dialog view.
    /// </summary>
    public interface IWindowView : IView
    {
        /// <summary>
        ///     Shows dialog as a window.
        /// </summary>
        void Show();

        /// <summary>
        ///     Shows dialog as a window.
        /// </summary>
        void ShowDialog();

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        void Close();

        /// <summary>
        ///     Occurred on closing.
        /// </summary>
        event EventHandler<object, CancelEventArgs> Closing;

        /// <summary>
        ///     Occurred on closed.
        /// </summary>
        event EventHandler<object, EventArgs> Closed;
    }
}