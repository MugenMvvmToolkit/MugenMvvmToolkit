using System;
using Foundation;
using MugenMvvm.Enums;
using MugenMvvm.Ios.Enums;
using MugenMvvm.Ios.Requests;
using MugenMvvm.Requests;
using UIKit;

namespace MugenMvvm.Ios.App
{
    public abstract class MugenApplicationDelegate : UIApplicationDelegate
    {
        #region Properties

        public override UIWindow? Window { get; set; }

        #endregion

        #region Methods

        public override void DidEnterBackground(UIApplication application)
        {
            MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivating, application);
            MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, application);
        }

        public override void OnActivated(UIApplication application)
        {
            MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Activating, application);
            MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Activated, application);
        }

        public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Initialize(application, launchOptions);
            return true;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            MugenService.Presenter.TryShow(application);
            return true;
        }

        public override bool ShouldSaveApplicationState(UIApplication application, NSCoder coder)
        {
            var request = new CancelableRequest(null, coder);
            MugenService.Application.OnLifecycleChanged(IosApplicationLifecycleState.Preserving, request);
            return !request.Cancel.GetValueOrDefault(true);
        }

        public override void WillEncodeRestorableState(UIApplication application, NSCoder coder) => MugenService.Application.OnLifecycleChanged(IosApplicationLifecycleState.Preserved, coder);

        public override bool ShouldRestoreApplicationState(UIApplication application, NSCoder coder)
        {
            try
            {
                var request = new CancelableRequest(null, coder);
                MugenService.Application.OnLifecycleChanged(IosApplicationLifecycleState.Restoring, request);
                return !request.Cancel.GetValueOrDefault(true);
            }
            catch (Exception e)
            {
                MugenService.Application.OnUnhandledException(e, UnhandledExceptionType.System);
                return false;
            }
        }

        public override UIViewController? GetViewController(UIApplication application, string[] restorationIdentifierComponents, NSCoder coder)
        {
            try
            {
                var request = new RestoreViewControllerRequest(coder, restorationIdentifierComponents);
                MugenService.Application.OnLifecycleChanged(IosApplicationLifecycleState.RestoringViewController, request);
                if (request.ViewController != null)
                    MugenService.Application.OnLifecycleChanged(IosApplicationLifecycleState.RestoredViewController, request.ViewController);

                return request.ViewController;
            }
            catch (Exception e)
            {
                MugenService.Application.OnUnhandledException(e, UnhandledExceptionType.System);
                return null;
            }
        }

        public override void DidDecodeRestorableState(UIApplication application, NSCoder coder)
        {
            try
            {
                MugenService.Application.OnLifecycleChanged(IosApplicationLifecycleState.Restored, coder);
            }
            catch (Exception e)
            {
                MugenService.Application.OnUnhandledException(e, UnhandledExceptionType.System);
            }
        }

        protected abstract void Initialize(UIApplication application, NSDictionary launchOptions);

        #endregion
    }
}