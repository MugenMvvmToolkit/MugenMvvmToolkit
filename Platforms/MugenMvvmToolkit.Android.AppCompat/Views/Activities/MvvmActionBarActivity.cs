#region Copyright

// ****************************************************************************
// <copyright file="MvvmActionBarActivity.cs">
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
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.AppCompat.Views.Activities
{
    public abstract class MvvmActionBarActivity : ActionBarActivity, IActivityView
    {
        #region Fields

        private IMvvmActivityMediator _mediator;
        private readonly int? _viewId;

        #endregion

        #region Constructors

        protected MvvmActionBarActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected MvvmActionBarActivity(int? viewId)
        {
            _viewId = viewId;
        }

        #endregion

        #region Implementation of IView

        /// <summary>
        ///     Gets the current <see cref="IMvvmActivityMediator" />.
        /// </summary>
        public virtual IMvvmActivityMediator Mediator
        {
            get { return this.GetOrCreateMediator(ref _mediator); }
        }

        /// <summary>
        ///     Gets or sets the data context of the current view.
        /// </summary>
        public object DataContext
        {
            get { return Mediator.DataContext; }
            set { Mediator.DataContext = value; }
        }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        public event EventHandler<Activity, EventArgs> DataContextChanged
        {
            add { Mediator.DataContextChanged += value; }
            remove { Mediator.DataContextChanged -= value; }
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

        public override void SetContentView(int layoutResID)
        {
            Mediator.SetContentView(layoutResID);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return Mediator.OnOptionsItemSelected(item, base.OnOptionsItemSelected);
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            Mediator.OnConfigurationChanged(newConfig, base.OnConfigurationChanged);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return Mediator.OnCreateOptionsMenu(menu, base.OnCreateOptionsMenu);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Mediator.OnCreate(savedInstanceState, base.OnCreate);
            if (_viewId.HasValue)
                SetContentView(_viewId.Value);
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

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            Mediator.OnPostCreate(savedInstanceState, base.OnPostCreate);
        }

        #endregion
    }
}