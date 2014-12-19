#region Copyright
// ****************************************************************************
// <copyright file="SplashScreenActivityBase.cs">
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
using System.Threading;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Views.Activities
{
    public abstract class SplashScreenActivityBase : Activity
    {
        #region Fields

        private const int DefaultState = 0;
        private const int StartedState = 1;
        private static AndroidBootstrapperBase _bootstrapper;
        private readonly int? _viewId;
        private static int _state;

        #endregion

        #region Constructors

        protected SplashScreenActivityBase(int? viewId = null)
        {
            _viewId = viewId;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current splash screen activity.
        /// </summary>
        [CanBeNull]
        public static SplashScreenActivityBase Current { get; private set; }

        #endregion

        #region Overrides of Activity

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Application.SynchronizationContext.Post(SetContentViewAsync, this);
            if (Interlocked.Exchange(ref _state, StartedState) == DefaultState)
            {
                Current = this;
                if (_bootstrapper == null)
                    ThreadPool.QueueUserWorkItem(StartBootstrapperCallback, this);
                else
                    StartBootstrapperCallback(this);
            }
        }

        #endregion

        #region Methods

        private static void SetContentViewAsync(object state)
        {
            var activity = (SplashScreenActivityBase)state;
            View content = activity._viewId.HasValue
                ? activity.LayoutInflater.Inflate(activity._viewId.Value, null)
                : activity.CreateDefaultView();
            if (content != null)
                activity.SetContentView(content);
        }

        private static void StartBootstrapperCallback(object state)
        {
            var activityBase = (SplashScreenActivityBase)state;
            Exception exception = null;
            try
            {
                if (_bootstrapper == null)
                {
                    _bootstrapper = activityBase.CreateBootstrapper();
                    _bootstrapper.InitializationContext = activityBase.GetContext() ?? DataContext.Empty;
                }
                _bootstrapper.Start();
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
                exception = e;
            }
            finally
            {
                activityBase.OnBootstrapperStarted(_bootstrapper, exception);
                Current = null;
                _state = DefaultState;
            }
        }

        [NotNull]
        protected abstract AndroidBootstrapperBase CreateBootstrapper();

        protected virtual void OnBootstrapperStarted([CanBeNull] AndroidBootstrapperBase bootstrapper, [CanBeNull] Exception exception)
        {
            if (exception != null)
                throw exception;
        }

        [CanBeNull]
        protected virtual IDataContext GetContext()
        {
            return DataContext.Empty;
        }

        [CanBeNull]
        protected virtual View CreateDefaultView()
        {
            var mainLayoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent);
            var layout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = mainLayoutParams
            };

            var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent, 1)
            {
                Gravity = GravityFlags.Bottom | GravityFlags.CenterHorizontal
            };
            var textView = new TextView(this)
            {
                Gravity = layoutParams.Gravity,
                LayoutParameters = layoutParams,
                Text = GetApplicationName()
            };
            textView.SetTextSize(ComplexUnitType.Dip, 30);
            layout.AddView(textView);

            layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.Center
            };
            //NOTE using App context to set platform specific theme for progressbar
            var bar = new ProgressBar(Application)
            {
                LayoutParameters = layoutParams,
                Indeterminate = true
            };
            layout.AddView(bar);

            layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent, 1)
            {
                Gravity = GravityFlags.Bottom | GravityFlags.CenterHorizontal
            };
            textView = new TextView(this)
            {
                Gravity = layoutParams.Gravity,
                LayoutParameters = layoutParams,
                Text = GetVersion()
            };
            layout.AddView(textView);

            layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.Bottom | GravityFlags.CenterHorizontal
            };
            textView = new TextView(this)
            {
                Gravity = layoutParams.Gravity,
                LayoutParameters = layoutParams,
                Text = GetFooter()
            };
            layout.AddView(textView);
            return layout;
        }

        protected virtual string GetApplicationName()
        {
            return Title;
        }

        protected virtual string GetVersion()
        {
            return "Version " + GetType().Assembly.GetName().Version;
        }

        protected virtual string GetFooter()
        {
            return "Powered by MUGEN MVVM TOOLKIT";
        }

        #endregion
    }
}