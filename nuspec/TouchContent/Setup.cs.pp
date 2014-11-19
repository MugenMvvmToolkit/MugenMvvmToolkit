using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit;
using MugenMvvmToolkit.Infrastructure;

namespace $rootnamespace$
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        #region Fields

        private const string RootViewControllerKey = "RootViewControllerKey";
        private TouchBootstrapperBase _bootstrapper;
        private UIWindow _window;

        #endregion

        #region Overrides of UIApplicationDelegate

        public override void WillEncodeRestorableState(UIApplication application, NSCoder coder)
        {
            if (_window.RootViewController != null)
                coder.Encode(_window.RootViewController, RootViewControllerKey);
        }

        public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // create a new window instance based on the screen size
            _window = new UIWindow(UIScreen.MainScreen.Bounds);
            _bootstrapper = new Bootstrapper<MainViewModel>(_window, new IIocContainer());
            _bootstrapper.Initialize();
            return true;
        }

        public override void DidDecodeRestorableState(UIApplication application, NSCoder coder)
        {
            var controller = (UIViewController)coder.DecodeObject(RootViewControllerKey);
            if (controller != null)
                _window.RootViewController = controller;
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            if (_window.RootViewController == null)
                _bootstrapper.Start();

            // make the window visible
            _window.MakeKeyAndVisible();

            return true;
        }

        public override UIViewController GetViewController(UIApplication application,
            string[] restorationIdentifierComponents, NSCoder coder)
        {
            return PlatformExtensions.ApplicationStateManager.GetViewController(restorationIdentifierComponents, coder);
        }

        public override bool ShouldRestoreApplicationState(UIApplication application, NSCoder coder)
        {
            return true;
        }

        public override bool ShouldSaveApplicationState(UIApplication application, NSCoder coder)
        {
            return true;
        }

        #endregion
    }
}