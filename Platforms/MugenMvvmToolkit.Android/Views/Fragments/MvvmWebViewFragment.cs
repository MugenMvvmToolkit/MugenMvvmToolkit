#region Copyright

// ****************************************************************************
// <copyright file="MvvmWebViewFragment.cs">
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
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Models;

#if APPCOMPAT
using MugenMvvmToolkit.AppCompat.Interfaces.Mediators;
using MugenMvvmToolkit.AppCompat.Interfaces.Views;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Fragment = Android.Support.V4.App.Fragment;
using WebViewFragment = Android.Support.V4.App.WebViewFragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace MugenMvvmToolkit.AppCompat.Views.Fragments
#else

namespace MugenMvvmToolkit.Android.Views.Fragments
#endif
{
    public abstract class MvvmWebViewFragment : WebViewFragment, IFragmentView
    {
        #region Fields

        private readonly int? _viewId;
        private IMvvmFragmentMediator _mediator;

        #endregion

        #region Constructors

        protected MvvmWebViewFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected MvvmWebViewFragment(int? viewId)
        {
            _viewId = viewId;
        }

        #endregion

        #region Implementation of IView

        public virtual IMvvmFragmentMediator Mediator
        {
            get { return this.GetOrCreateMediator(ref _mediator); }
        }

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
            return Mediator.OnCreateView(_viewId, inflater, container, savedInstanceState, base.OnCreateView);
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

        #endregion
    }
}
