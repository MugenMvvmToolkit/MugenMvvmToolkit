using System;
using System.Collections.Generic;
using Foundation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Ios.Enums;
using UIKit;

namespace MugenMvvm.Ios.Views
{
    public class MugenViewController : UIViewController, IValueHolder<IDictionary<string, object?>>, IValueHolder<IWeakReference>
    {
        public MugenViewController()
        {
        }

        public MugenViewController(NSCoder coder) : base(coder)
        {
        }

        public MugenViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
        }

        protected internal MugenViewController(IntPtr handle) : base(handle)
        {
        }

        protected MugenViewController(NSObjectFlag t) : base(t)
        {
        }

        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

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
    }
}