#region Copyright
// ****************************************************************************
// <copyright file="MvvmListActivity.cs">
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
using System.Threading;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Views.Activities
{
    public abstract class MvvmListActivity : Activity, IActivityView
    {
        #region Fields

        private IMvvmActivityMediator _mediator;
        private readonly int? _viewId;

        #endregion

        #region Constructors

        protected MvvmListActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected MvvmListActivity(int? viewId)
        {
            _viewId = viewId;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="IMvvmActivityMediator" />.
        /// </summary>
        protected IMvvmActivityMediator Mediator
        {
            get
            {
                if (_mediator == null)
                    Interlocked.CompareExchange(ref _mediator, PlatformExtensions.MvvmActivityMediatorFactory(this, Models.DataContext.Empty), null);
                return _mediator;
            }
        }

        #endregion

        #region Implementation of IView

        /// <summary>
        ///     Gets or sets the data context of the current <see cref="IView" />.
        /// </summary>
        public object DataContext
        {
            get { return Mediator.DataContext; }
            set { Mediator.DataContext = value; }
        }

        /// <summary>
        /// Gets the current bundle.
        /// </summary>
        public Bundle Bundle
        {
            get { return Mediator.Bundle; }
        }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        public event EventHandler<Activity, EventArgs> DataContextChanged
        {
            add { Mediator.DataContextChanged += value; }
            remove { Mediator.DataContextChanged -= value; }
        }

        /// <summary>
        /// Occurs when the activity has detected the user's press of the back key.
        /// </summary>
        public event EventHandler<Activity, CancelEventArgs> BackPressing
        {
            add { Mediator.BackPressing += value; }
            remove { Mediator.BackPressing -= value; }
        }

        /// <summary>
        ///     Occurred on created activity.
        /// </summary>
        public event EventHandler<Activity, ValueEventArgs<Bundle>> Created
        {
            add { Mediator.Created += value; }
            remove { Mediator.Created -= value; }
        }

        /// <summary>
        ///     Occurred on started activity.
        /// </summary>
        public event EventHandler<Activity, EventArgs> Started
        {
            add { Mediator.Started += value; }
            remove { Mediator.Started -= value; }
        }

        /// <summary>
        ///     Occurred on paused activity.
        /// </summary>
        public event EventHandler<Activity, EventArgs> Paused
        {
            add { Mediator.Paused += value; }
            remove { Mediator.Paused -= value; }
        }

        /// <summary>
        ///     Occurred on save activity state.
        /// </summary>
        public event EventHandler<Activity, ValueEventArgs<Bundle>> SaveInstanceState
        {
            add { Mediator.SaveInstanceState += value; }
            remove { Mediator.SaveInstanceState -= value; }
        }

        /// <summary>
        ///     Occurred on stoped activity.
        /// </summary>
        public event EventHandler<Activity, EventArgs> Stoped
        {
            add { Mediator.Stoped += value; }
            remove { Mediator.Stoped -= value; }
        }

        /// <summary>
        ///     Occurred on restarted activity.
        /// </summary>
        public event EventHandler<Activity, EventArgs> Restarted
        {
            add { Mediator.Restarted += value; }
            remove { Mediator.Restarted -= value; }
        }

        /// <summary>
        ///     Occurred on resume activity.
        /// </summary>
        public event EventHandler<Activity, EventArgs> Resume
        {
            add { Mediator.Resume += value; }
            remove { Mediator.Resume -= value; }
        }

        /// <summary>
        ///     Occurred on destroyed activity.
        /// </summary>
        public event EventHandler<Activity, EventArgs> Destroyed
        {
            add { Mediator.Destroyed += value; }
            remove { Mediator.Destroyed -= value; }
        }

        #endregion

        #region Overrides of Activity

        public override MenuInflater MenuInflater
        {
            get { return Mediator.GetMenuInflater(base.MenuInflater); }
        }

        public override void Finish()
        {
            Mediator.Finish(base.Finish);
        }

        public override void OnBackPressed()
        {
            Mediator.OnBackPressed(base.OnBackPressed);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Mediator.OnCreate(savedInstanceState, base.OnCreate);
            if (_viewId.HasValue)
                SetContentView(_viewId.Value);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return Mediator.OnCreateOptionsMenu(menu, base.OnCreateOptionsMenu);
        }

        protected override void OnDestroy()
        {
            Mediator.OnDestroy(base.OnDestroy);
        }

        protected override void OnPause()
        {
            Mediator.OnPause(base.OnPause);
        }

        protected override void OnRestart()
        {
            Mediator.OnRestart(base.OnRestart);
        }

        protected override void OnResume()
        {
            Mediator.OnResume(base.OnResume);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            Mediator.OnSaveInstanceState(outState, base.OnSaveInstanceState);
        }

        protected override void OnStart()
        {
            Mediator.OnStart(base.OnStart);
        }

        protected override void OnStop()
        {
            Mediator.OnStop(base.OnStop);
        }

        public override void SetContentView(int layoutResID)
        {
            Mediator.SetContentView(layoutResID);
        }

        #endregion
    }
}