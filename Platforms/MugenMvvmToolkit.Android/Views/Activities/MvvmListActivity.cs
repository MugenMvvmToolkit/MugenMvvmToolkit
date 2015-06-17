#region Copyright

// ****************************************************************************
// <copyright file="MvvmListActivity.cs">
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
using Android.Views;
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.Views.Activities
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

        public override LayoutInflater LayoutInflater
        {
            get { return Mediator.GetLayoutInflater(base.LayoutInflater); }
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
            Mediator.OnCreate(_viewId, savedInstanceState, base.OnCreate);
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