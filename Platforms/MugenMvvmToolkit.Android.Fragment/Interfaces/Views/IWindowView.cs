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
using Android.App;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
#if APPCOMPAT
using MugenMvvmToolkit.AppCompat.Interfaces.Views;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace MugenMvvmToolkit.AppCompat.Interfaces.Views
#else
namespace MugenMvvmToolkit.FragmentSupport.Interfaces.Views
#endif
{
    /// <summary>
    ///     Represent the base interface for a window view.
    /// </summary>
    public interface IWindowView : IFragmentView
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
    }
}