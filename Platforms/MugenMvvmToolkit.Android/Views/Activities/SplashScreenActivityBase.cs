#region Copyright

// ****************************************************************************
// <copyright file="SplashScreenActivityBase.cs">
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
using System.Threading;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Infrastructure;

namespace MugenMvvmToolkit.Android.Views.Activities
{
    public abstract class SplashScreenActivityBase : Activity
    {
        #region Fields

        private readonly int? _viewId;

        private const int DefaultState = 0;
        private const int StartedState = 1;
        private static int _state;

        #endregion

        #region Constructors

        protected SplashScreenActivityBase(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected SplashScreenActivityBase(int? viewId = null)
        {
            _viewId = viewId;
        }

        #endregion

        #region Methods

        #region Overrides of Activity

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var content = _viewId.HasValue
                ? LayoutInflater.Inflate(_viewId.Value, null)
                : CreateDefaultView();
            if (content != null)
                SetContentView(content);

            if (Interlocked.Exchange(ref _state, StartedState) == DefaultState)
            {
                PlatformExtensions.SetCurrentActivity(this, false);
                if (AndroidBootstrapperBase.Current != null && AndroidBootstrapperBase.Current.IsInitialized)
                    StartBootstrapperCallback(this);
                else
                    ThreadPool.QueueUserWorkItem(StartBootstrapperCallback, this);
            }
        }

        #endregion

        private static void StartBootstrapperCallback(object state)
        {
            var activityBase = (SplashScreenActivityBase)state;
            Exception exception = null;
            AndroidBootstrapperBase bootstrapper = null;
            try
            {
                bootstrapper = AndroidBootstrapperBase.GetOrCreateBootstrapper(activityBase.CreateBootstrapper);
                bootstrapper.Start();
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
                exception = e;
            }
            finally
            {
                activityBase.OnBootstrapperStarted(bootstrapper, exception);
                PlatformExtensions.SetCurrentActivity(activityBase, true);
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