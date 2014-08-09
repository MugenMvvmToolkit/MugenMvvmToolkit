#region Copyright
// ****************************************************************************
// <copyright file="IWindowView.cs">
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
using System.ComponentModel;
using Android.App;
using Android.Support.V4.App;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Views
{
    /// <summary>
    ///     Represent the base interface for a window view.
    /// </summary>
    public interface IWindowView : IWindowViewBase
    {
        /// <summary>
        ///     Gets or sets the cancelable value.
        /// </summary>
        bool Cancelable { get; set; }

        /// <summary>
        ///     Shows the window.
        /// </summary>
        void Show(FragmentManager manager, string tag);

        /// <summary>
        ///     Dismiss the window
        /// </summary>
        void Dismiss();

        /// <summary>
        ///     Occurred on closing window.
        /// </summary>
        event EventHandler<IWindowView, CancelEventArgs> Closing;

        /// <summary>
        ///     Occurred on closed window.
        /// </summary>
        event EventHandler<IWindowView, EventArgs> Canceled;
    }
}