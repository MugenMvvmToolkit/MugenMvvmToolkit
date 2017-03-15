#region Copyright

// ****************************************************************************
// <copyright file="MvvmPreferenceFragment.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.Views.Fragments
{
    public abstract class MvvmPreferenceFragment : PreferenceFragment, IFragmentView
    {
        #region Fields

        private readonly int? _viewId;
        private IMvvmFragmentMediator _mediator;

        #endregion

        #region Constructors

        protected MvvmPreferenceFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected MvvmPreferenceFragment(int? viewId = null)
        {
            _viewId = viewId;
        }

        #endregion

        #region Implementation of IView

        public virtual IMvvmFragmentMediator Mediator => this.GetOrCreateMediator(ref _mediator);

        public object DataContext
        {
            get { return Mediator.DataContext; }
            set { Mediator.DataContext = value; }
        }

        public event EventHandler<Fragment, EventArgs> DataContextChanged
        {
            add { Mediator.DataContextChanged += value; }
            remove { Mediator.DataContextChanged -= value; }
        }

        #endregion

        #region Properties

        protected virtual int? ViewId => _viewId;

        #endregion

        #region Overrides of Fragment

        public override void OnAttach(Activity activity)
        {
            Mediator.OnAttach(activity, base.OnAttach);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            Mediator.OnCreate(savedInstanceState, base.OnCreate);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            Mediator.OnCreateOptionsMenu(menu, inflater, base.OnCreateOptionsMenu);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return Mediator.OnCreateView(ViewId, inflater, container, savedInstanceState, base.OnCreateView);
        }

        public override void OnDestroyView()
        {
            Mediator.OnDestroyView(base.OnDestroyView);
        }

        public override void OnDestroy()
        {
            Mediator.OnDestroy(base.OnDestroy);
        }

        public override void OnDetach()
        {
            Mediator.OnDetach(base.OnDetach);
        }

        public override void OnInflate(Activity activity, IAttributeSet attrs, Bundle savedInstanceState)
        {
            Mediator.OnInflate(activity, attrs, savedInstanceState, base.OnInflate);
        }

        public override void OnPause()
        {
            Mediator.OnPause(base.OnPause);
        }

        public override void OnResume()
        {
            Mediator.OnResume(base.OnResume);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            Mediator.OnSaveInstanceState(outState, base.OnSaveInstanceState);
        }

        public override void OnStart()
        {
            Mediator.OnStart(base.OnStart);
        }

        public override void OnStop()
        {
            Mediator.OnStop(base.OnStop);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            Mediator.OnViewCreated(view, savedInstanceState, base.OnViewCreated);
        }

        public override void AddPreferencesFromResource(int preferencesResId)
        {
            Mediator.AddPreferencesFromResource(base.AddPreferencesFromResource, preferencesResId);
        }

        #endregion
    }
}
