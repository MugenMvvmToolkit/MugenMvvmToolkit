#region Copyright

// ****************************************************************************
// <copyright file="MvvmNavigationController.cs">
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
using System.ComponentModel;
using Foundation;
using MugenMvvmToolkit.iOS.Interfaces.Mediators;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.Models;
using UIKit;

namespace MugenMvvmToolkit.iOS.Views
{
    [Register("MvvmNavigationController")]
    public class MvvmNavigationController : UINavigationController, IMvvmNavigationController
    {
        #region Fields

        private IMvvmViewControllerMediator _mediator;

        #endregion

        #region Constructors

        public MvvmNavigationController(Type navigationBarType, Type toolbarType)
            : base(navigationBarType, toolbarType)
        {
        }

        public MvvmNavigationController()
        {
        }

        public MvvmNavigationController(NSCoder coder)
            : base(coder)
        {
        }

        public MvvmNavigationController(NSObjectFlag t)
            : base(t)
        {
        }

        public MvvmNavigationController(IntPtr handle)
            : base(handle)
        {
        }

        public MvvmNavigationController(string nibName, NSBundle bundle)
            : base(nibName, bundle)
        {
        }

        public MvvmNavigationController(UIViewController rootViewController)
            : base(rootViewController)
        {
        }

        #endregion

        #region Overrides of UINavigationController

        public override UIViewController PopViewController(bool animated)
        {
            var handler = ShouldPopViewController;
            if (handler != null)
            {
                var args = new CancelEventArgs();
                handler(this, args);
                if (args.Cancel)
                {
                    if (NavigationBar.Subviews != null)
                    {
                        foreach (UIView view in NavigationBar.Subviews)
                        {
                            if (view.Alpha < 1)
                            {
                                UIView uiView = view;
                                UIView.Animate(0.25, () => uiView.Alpha = 1);
                            }
                        }
                    }
                    return null;
                }
            }
            return base.PopViewController(animated);
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            Mediator.DecodeRestorableState(base.DecodeRestorableState, coder);
        }

        public override void EncodeRestorableState(NSCoder coder)
        {
            Mediator.EncodeRestorableState(base.EncodeRestorableState, coder);
        }

        public override void ViewDidAppear(bool animated)
        {
            Mediator.ViewDidAppear(base.ViewDidAppear, animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            Mediator.ViewDidDisappear(base.ViewDidDisappear, animated);
        }

        public override void ViewDidLoad()
        {
            Mediator.ViewDidLoad(base.ViewDidLoad);
        }

        public override void ViewWillAppear(bool animated)
        {
            Mediator.ViewWillAppear(base.ViewWillAppear, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            Mediator.ViewWillDisappear(base.ViewWillDisappear, animated);
        }

        protected override void Dispose(bool disposing)
        {
            Mediator.Dispose(base.Dispose, disposing);
        }

        #endregion

        #region Methods

        [Export("navigationBar:shouldPopItem:")]
        protected virtual bool ShouldPopItem(UINavigationBar navigationBar, UINavigationItem item)
        {
            if (ViewControllers.Length < navigationBar.Items.Length)
                return true;
            PopViewController(true);
            return false;
        }

        [Export("navigationBar:didPopItem:")]
        protected virtual void DidPopItem(UINavigationBar navigationBar, UINavigationItem item)
        {
            RaiseDidPopViewController();
        }

        protected void RaiseDidPopViewController()
        {
            var handler = DidPopViewController;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Implementation of IMvvmNavigationController

        public virtual IMvvmViewControllerMediator Mediator
        {
            get { return this.GetOrCreateMediator(ref _mediator); }
        }

        public event EventHandler<IMvvmNavigationController, CancelEventArgs> ShouldPopViewController;

        public event EventHandler<IMvvmNavigationController, EventArgs> DidPopViewController;

        #endregion
    }
}
