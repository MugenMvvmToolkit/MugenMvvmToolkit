using System;
using System.Collections.Generic;
using Foundation;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Ios.Enums;
using MugenMvvm.Requests;
using UIKit;

namespace MugenMvvm.Ios.Views
{
    public class MugenNavigationController : UINavigationController, IValueHolder<IDictionary<string, object?>>, IValueHolder<IWeakReference>
    {
        #region Constructors

        public MugenNavigationController(Type navigationBarType, Type toolbarType) : base(navigationBarType, toolbarType)
        {
        }

        public MugenNavigationController()
        {
        }

        public MugenNavigationController(NSCoder coder) : base(coder)
        {
        }

        public MugenNavigationController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
        }

        public MugenNavigationController(UIViewController rootViewController) : base(rootViewController)
        {
        }

        protected MugenNavigationController(NSObjectFlag t) : base(t)
        {
        }

        protected internal MugenNavigationController(IntPtr handle) : base(handle)
        {
        }

        #endregion

        #region Properties

        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        #endregion

        #region Methods

        public override void DidMoveToParentViewController(UIViewController parent)
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.DidMovingToParentViewController, parent);
            base.DidMoveToParentViewController(parent);
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.DidMovedToParentViewController, parent);
        }

        public override void RemoveFromParentViewController()
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.RemovingFromParentViewController);
            base.RemoveFromParentViewController();
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.RemovedFromParentViewController);
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.WillMovingToParentViewController, parent);
            base.WillMoveToParentViewController(parent);
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.WillMovedToParentViewController, parent);
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.DecodingRestorableState, coder);
            base.DecodeRestorableState(coder);
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.DecodedRestorableState, coder);
        }

        public override void EncodeRestorableState(NSCoder coder)
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.EncodingRestorableState, coder);
            base.EncodeRestorableState(coder);
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.EncodedRestorableState, coder);
        }

        public override void ViewDidLoad()
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.DidLoading);
            base.ViewDidLoad();
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.DidLoaded);
        }

        public override void ViewWillAppear(bool animated)
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.WillAppearing);
            base.ViewWillAppear(animated);
            MugenService.ViewManager.OnLifecycleChanged(this, ViewLifecycleState.Appearing);
        }

        public override void ViewDidAppear(bool animated)
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.WillAppeared);
            base.ViewDidAppear(animated);
            MugenService.ViewManager.OnLifecycleChanged(this, ViewLifecycleState.Appeared);
        }

        public override void ViewWillDisappear(bool animated)
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.WillDisappearing);
            base.ViewWillDisappear(animated);
            MugenService.ViewManager.OnLifecycleChanged(this, ViewLifecycleState.Disappearing);
        }

        public override void ViewDidDisappear(bool animated)
        {
            MugenService.ViewManager.OnLifecycleChanged(this, IosViewLifecycleState.WillDisappeared);
            base.ViewDidDisappear(animated);
            MugenService.ViewManager.OnLifecycleChanged(this, ViewLifecycleState.Disappeared);
        }

        public override UIViewController PopViewController(bool animated)
        {
            var controllers = ViewControllers;
            if (controllers == null || controllers.Length == 0)
                return base.PopViewController(animated);

            var controller = controllers[controllers.Length - 1];
            var request = new CancelableRequest(null, this);
            MugenService.ViewManager.OnLifecycleChanged(controller, ViewLifecycleState.Closing, request);
            if (!request.Cancel.GetValueOrDefault())
            {
                var popViewController = base.PopViewController(animated);
                MugenService.ViewManager.OnLifecycleChanged(controller, ViewLifecycleState.Closed, request);
                return popViewController;
            }

            var subviews = NavigationBar.Subviews;
            if (subviews != null)
            {
                foreach (UIView view in subviews)
                {
                    if (view.Alpha < 1)
                    {
                        UIView uiView = view;
                        UIView.Animate(0.25, () => uiView.Alpha = 1);
                    }
                }
            }

            return null!;
        }

        [Export("navigationBar:shouldPopItem:")]
        protected virtual bool ShouldPopItem(UINavigationBar navigationBar, UINavigationItem item)
        {
            var viewControllers = ViewControllers;
            if (viewControllers != null && viewControllers.Length < navigationBar.Items.Length)
                return true;
            PopViewController(true);
            return false;
        }

        #endregion
    }
}