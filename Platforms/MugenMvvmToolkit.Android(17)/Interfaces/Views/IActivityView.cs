#region Copyright

// ****************************************************************************
// <copyright file="IActivityView.cs">
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
using Android.OS;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Views
{
    public interface IActivityView : IView
    {
        /// <summary>
        ///     Gets or sets the data context of the current view.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        ///     Returns true if the final <c><see cref="M:Android.App.Activity.OnDestroy" /></c> call has been made on the Activity, so this instance is now dead.
        /// </summary>
        bool IsDestroyed { get; }

        /// <summary>
        ///     Gets the current bundle.
        /// </summary>
        [CanBeNull]
        Bundle Bundle { get; }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        event EventHandler<Activity, EventArgs> DataContextChanged;

        /// <summary>
        ///     Occurs when the activity has detected the user's press of the back key.
        /// </summary>
        event EventHandler<Activity, CancelEventArgs> BackPressing;

        /// <summary>
        ///     Occurred on created activity.
        /// </summary>
        event EventHandler<Activity, ValueEventArgs<Bundle>> Created;

        /// <summary>
        ///     Occurred on started activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Started;

        /// <summary>
        ///     Occurred on paused activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Paused;

        /// <summary>
        ///     Occurred on save activity state.
        /// </summary>
        event EventHandler<Activity, ValueEventArgs<Bundle>> SaveInstanceState;

        /// <summary>
        ///     Occurred on stoped activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Stoped;

        /// <summary>
        ///     Occurred on restarted activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Restarted;

        /// <summary>
        ///     Occurred on resume activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Resume;

        /// <summary>
        ///     Occurred on destroyed activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Destroyed;
    }
}