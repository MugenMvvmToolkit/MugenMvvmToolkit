#region Copyright

// ****************************************************************************
// <copyright file="IActivityView.cs">
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
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.Interfaces.Views
{
    public interface IActivityView : IView
    {
        /// <summary>
        ///     Gets the current <see cref="IMvvmActivityMediator" />.
        /// </summary>
        [NotNull]
        IMvvmActivityMediator Mediator { get; }

        /// <summary>
        ///     Gets or sets the data context of the current view.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        event EventHandler<Activity, EventArgs> DataContextChanged;
    }
}