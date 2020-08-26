using Foundation;
using MugenMvvm.Enums;
using UIKit;

namespace MugenMvvm.Ios.App
{
    public abstract class MugenApplicationDelegate : UIApplicationDelegate
    {
        #region Fields

        private bool _isInitialized;

        #endregion

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

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            InitializeIfNeed();
            MugenService.Presenter.TryShow(application);
            return true;
        }

        // public override bool ShouldRestoreApplicationState(UIApplication application, NSCoder coder) => base.ShouldRestoreApplicationState(application, coder);
        //
        // public override void DidDecodeRestorableState(UIApplication application, NSCoder coder) => base.DidDecodeRestorableState(application, coder);
        //
        // public override bool ShouldSaveApplicationState(UIApplication application, NSCoder coder) => base.ShouldSaveApplicationState(application, coder);
        //
        // public override void WillEncodeRestorableState(UIApplication application, NSCoder coder) => base.WillEncodeRestorableState(application, coder);
        //
        // public override UIViewController GetViewController(UIApplication application, string[] restorationIdentifierComponents, NSCoder coder) =>
        //     base.GetViewController(application, restorationIdentifierComponents, coder);

        protected void InitializeIfNeed()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }
        }

        protected abstract void Initialize();

        #endregion
    }
}