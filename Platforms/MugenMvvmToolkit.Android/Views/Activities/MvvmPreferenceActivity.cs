#region Copyright

// ****************************************************************************
// <copyright file="MvvmPreferenceActivity.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.Views.Activities
{
    public abstract class MvvmPreferenceActivity : PreferenceActivity, IActivityView
    {
        #region Fields

        private IMvvmActivityMediator _mediator;
        private readonly int? _viewId;

        #endregion

        #region Constructors

        protected MvvmPreferenceActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected MvvmPreferenceActivity(int? viewId = null)
        {
            _viewId = viewId;
        }

        #endregion

        #region Implementation of IView

        public virtual IMvvmActivityMediator Mediator => this.GetOrCreateMediator(ref _mediator);

        public object DataContext
        {
            get { return Mediator.DataContext; }
            set { Mediator.DataContext = value; }
        }

        public event EventHandler<Activity, EventArgs> DataContextChanged
        {
            add { Mediator.DataContextChanged += value; }
            remove { Mediator.DataContextChanged -= value; }
        }

        #endregion

        #region Properties

        protected virtual int? ViewId => _viewId;

        #endregion

        #region Overrides of Activity

        public override MenuInflater MenuInflater => Mediator.GetMenuInflater(base.MenuInflater);

        public override LayoutInflater LayoutInflater => Mediator.GetLayoutInflater(base.LayoutInflater);

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

        [Obsolete("deprecated")]
        public override void AddPreferencesFromResource(int preferencesResId)
        {
            Mediator.AddPreferencesFromResource(base.AddPreferencesFromResource, preferencesResId);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Mediator.OnCreate(ViewId, savedInstanceState, base.OnCreate);
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
