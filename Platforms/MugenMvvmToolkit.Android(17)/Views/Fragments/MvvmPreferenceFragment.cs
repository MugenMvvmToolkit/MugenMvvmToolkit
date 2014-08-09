#region Copyright
// ****************************************************************************
// <copyright file="MvvmPreferenceFragment.cs">
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
using Android.App;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Android.Views;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Views.Fragments
{
    public abstract class MvvmPreferenceFragment : PreferenceFragment, IView
    {
        #region Fields

        private readonly IMvvmFragmentMediator _mediator;
        private readonly int? _viewId;

        #endregion

        #region Constructors

        protected MvvmPreferenceFragment(int? viewId)
        {
            _viewId = viewId;
            _mediator = PlatformExtensions.MvvmFragmentMediatorFactory(this, Models.DataContext.Empty);
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
        ///     Occurs when the DataContext property changed.
        /// </summary>
        public virtual event EventHandler<Fragment, EventArgs> DataContextChanged
        {
            add { _mediator.DataContextChanged += value; }
            remove { _mediator.DataContextChanged -= value; }
        }

        #endregion

        #region Overrides of Fragment

        public override void OnAttach(Activity activity)
        {
            _mediator.OnAttach(activity, base.OnAttach);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            _mediator.OnCreate(savedInstanceState, base.OnCreate);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            _mediator.OnCreateOptionsMenu(menu, inflater, base.OnCreateOptionsMenu);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return _mediator.OnCreateView(_viewId, inflater, container, savedInstanceState, base.OnCreateView);
        }

        public override void OnDestroy()
        {
            _mediator.OnDestroy(base.OnDestroy);
        }

        public override void OnDetach()
        {
            _mediator.OnDetach(base.OnDetach);
        }

        public override void OnInflate(Activity activity, IAttributeSet attrs, Bundle savedInstanceState)
        {
            _mediator.OnInflate(activity, attrs, savedInstanceState, base.OnInflate);
        }

        public override void OnPause()
        {
            _mediator.OnPause(base.OnPause);
        }

        public override void OnResume()
        {
            _mediator.OnResume(base.OnResume);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            _mediator.OnSaveInstanceState(outState, base.OnSaveInstanceState);
        }

        public override void OnStart()
        {
            _mediator.OnStart(base.OnStart);
        }

        public override void OnStop()
        {
            _mediator.OnStop(base.OnStop);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            _mediator.OnViewCreated(view, savedInstanceState, base.OnViewCreated);
        }

        #endregion
    }
}