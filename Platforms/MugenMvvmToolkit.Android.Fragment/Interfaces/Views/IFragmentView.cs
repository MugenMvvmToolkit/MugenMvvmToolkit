#region Copyright

// ****************************************************************************
// <copyright file="IFragmentView.cs">
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
using Android.App;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
#if APPCOMPAT
using Fragment = Android.Support.V4.App.Fragment;
using MugenMvvmToolkit.AppCompat.Interfaces.Mediators;

namespace MugenMvvmToolkit.AppCompat.Interfaces.Views
#else
using MugenMvvmToolkit.FragmentSupport.Interfaces.Mediators;

namespace MugenMvvmToolkit.FragmentSupport.Interfaces.Views
#endif
{
    public interface IFragmentView : IView
    {
        /// <summary>
        ///     Gets the current <see cref="IMvvmFragmentMediator" />.
        /// </summary>
        [NotNull]
        IMvvmFragmentMediator Mediator { get; }

        /// <summary>
        ///     Gets or sets the data context of the current view.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        event EventHandler<Fragment, EventArgs> DataContextChanged;
    }
}