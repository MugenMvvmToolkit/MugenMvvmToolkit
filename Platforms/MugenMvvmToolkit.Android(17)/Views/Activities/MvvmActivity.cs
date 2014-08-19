#region Copyright
// ****************************************************************************
// <copyright file="MvvmActivity.cs">
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
using Android.Views;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Views.Activities
{
    public abstract class MvvmActivity : Activity, IActivityView
    {
        #region Fields

        private readonly IMvvmActivityMediator _mediator;
        private readonly int? _viewId;

        #endregion

        #region Constructors

        protected MvvmActivity(int? viewId)
        {
            _viewId = viewId;
            _mediator = PlatformExtensions.MvvmActivityMediatorFactory(this, Models.DataContext.Empty);
        }

        #endregion

        #region Implementation of IView

        /// <summary>
        ///     Gets or sets the data context of the current <see cref="IView" />.
        /// </summary>
        public virtual object DataContext
        {
            get { return _mediator.DataContext; }
            set { _mediator.DataContext = value; }
        }

        /// <summary>
        /// Gets the current bundle.
        /// </summary>
        public Bundle Bundle
        {
            get { return _mediator.Bundle; }
        }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> DataContextChanged
        {
            add { _mediator.DataContextChanged += value; }
            remove { _mediator.DataContextChanged -= value; }
        }

        /// <summary>
        /// Occurs when the activity has detected the user's press of the back key.
        /// </summary>
        public virtual event EventHandler<Activity, CancelEventArgs> BackPressing
        {
            add { _mediator.BackPressing += value; }
            remove { _mediator.BackPressing -= value; }
        }

        /// <summary>
        ///     Occurred on created activity.
        /// </summary>
        public virtual event EventHandler<Activity, ValueEventArgs<Bundle>> Created
        {
            add { _mediator.Created += value; }
            remove { _mediator.Created -= value; }
        }

        /// <summary>
        ///     Occurred on started activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Started
        {
            add { _mediator.Started += value; }
            remove { _mediator.Started -= value; }
        }

        /// <summary>
        ///     Occurred on paused activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Paused
        {
            add { _mediator.Paused += value; }
            remove { _mediator.Paused -= value; }
        }

        /// <summary>
        ///     Occurred on save activity state.
        /// </summary>
        public event EventHandler<Activity, ValueEventArgs<Bundle>> SaveInstanceState
        {
            add { _mediator.SaveInstanceState += value; }
            remove { _mediator.SaveInstanceState -= value; }
        }

        /// <summary>
        ///     Occurred on stoped activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Stoped
        {
            add { _mediator.Stoped += value; }
            remove { _mediator.Stoped -= value; }
        }

        /// <summary>
        ///     Occurred on restarted activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Restarted
        {
            add { _mediator.Restarted += value; }
            remove { _mediator.Restarted -= value; }
        }

        /// <summary>
        ///     Occurred on resume activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Resume
        {
            add { _mediator.Resume += value; }
            remove { _mediator.Resume -= value; }
        }

        /// <summary>
        ///     Occurred on destroyed activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Destroyed
        {
            add { _mediator.Destroyed += value; }
            remove { _mediator.Destroyed -= value; }
        }

        #endregion

        #region Overrides of Activity

        public override MenuInflater MenuInflater
        {
            get { return _mediator.GetMenuInflater(base.MenuInflater); }
        }

        public override void Finish()
        {
            _mediator.Finish(base.Finish);
        }

        public override void OnBackPressed()
        {
            _mediator.OnBackPressed(base.OnBackPressed);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _mediator.OnCreate(savedInstanceState, base.OnCreate);
            if (_viewId.HasValue)
                SetContentView(_viewId.Value);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return _mediator.OnCreateOptionsMenu(menu, base.OnCreateOptionsMenu);
        }

        protected override void OnDestroy()
        {
            _mediator.OnDestroy(base.OnDestroy);
        }

        protected override void OnPause()
        {
            _mediator.OnPause(base.OnPause);
        }

        protected override void OnRestart()
        {
            _mediator.OnRestart(base.OnRestart);
        }

        protected override void OnResume()
        {
            _mediator.OnResume(base.OnResume);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            _mediator.OnSaveInstanceState(outState, base.OnSaveInstanceState);
        }

        protected override void OnStart()
        {
            _mediator.OnStart(base.OnStart);
        }

        protected override void OnStop()
        {
            _mediator.OnStop(base.OnStop);
        }

        public override void SetContentView(int layoutResID)
        {
            _mediator.SetContentView(layoutResID);
        }

        #endregion
    }
}