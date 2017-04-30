#region Copyright

// ****************************************************************************
// <copyright file="MvvmAppDelegateBase.cs">
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

using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.iOS.Infrastructure;
using MugenMvvmToolkit.Models.Messages;
using UIKit;

namespace MugenMvvmToolkit.iOS
{
    public abstract class MvvmAppDelegateBase : UIApplicationDelegate
    {
        #region Fields

        protected const string AppVersionKey = nameof(AppVersionKey);
        protected const string RootViewControllerKey = nameof(RootViewControllerKey);

        private TouchBootstrapperBase _bootstrapper;
        private bool _isRestored;
        private bool _isStarted;
        private static string _version;

        #endregion

        #region Properties

        public override UIWindow Window { get; set; }

        public static string Version
        {
            get
            {
                if (_version == null)
                {
                    try
                    {
                        var build = NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion"));
                        var version = NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleShortVersionString"));
                        _version = $"{version}.{build}";
                    }
                    catch
                    {
                        _version = string.Empty;
                    }
                }
                return _version;
            }
        }

        #endregion

        #region Methods

        public override void DidDecodeRestorableState(UIApplication application, NSCoder coder)
        {
            var controller = coder.DecodeObject(RootViewControllerKey) as UIViewController;
            if (controller != null)
            {
                _isRestored = true;
                Window.RootViewController = controller;
            }
        }

        public override void DidEnterBackground(UIApplication application)
        {
            ServiceProvider.EventAggregator.Publish(this, new BackgroundNavigationMessage());
        }

        public override void OnActivated(UIApplication application)
        {
            if (_isStarted)
                ServiceProvider.EventAggregator.Publish(this, new ForegroundNavigationMessage());
            else
                _isStarted = true;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            InitializeIfNeed();
            if (!_isRestored)
                _bootstrapper.Start();

            // make the window visible
            Window.MakeKeyAndVisible();
            return true;
        }

        public override UIViewController GetViewController(UIApplication application,
            string[] restorationIdentifierComponents, NSCoder coder)
        {
            InitializeIfNeed();
            return TouchToolkitExtensions.ApplicationStateManager.GetViewController(restorationIdentifierComponents, coder);
        }

        public override bool ShouldRestoreApplicationState(UIApplication application, NSCoder coder)
        {
            var version = Version;
            var oldVersion = (NSString)coder.DecodeObject(AppVersionKey);
            return oldVersion != null && oldVersion.ToString() == version;
        }

        public override bool ShouldSaveApplicationState(UIApplication application, NSCoder coder)
        {
            return true;
        }

        public override void WillEncodeRestorableState(UIApplication application, NSCoder coder)
        {
            var controller = Window.RootViewController;
            if (controller != null)
            {
                coder.Encode(controller, RootViewControllerKey);
                coder.Encode(new NSString(Version), AppVersionKey);
            }
        }

        public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // create a new window instance based on the screen size
            return true;
        }

        private void InitializeIfNeed()
        {
            if (_bootstrapper != null)
                return;
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            _bootstrapper = CreateBootstrapper(Window);
            _bootstrapper.Initialize();
        }

        [NotNull]
        protected abstract TouchBootstrapperBase CreateBootstrapper([NotNull]UIWindow window);

        #endregion
    }
}