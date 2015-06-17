#region Copyright

// ****************************************************************************
// <copyright file="MvvmTableViewController.cs">
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
using Foundation;
using MugenMvvmToolkit.iOS.Interfaces.Mediators;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using UIKit;

namespace MugenMvvmToolkit.iOS.Views
{
    public abstract class MvvmTableViewController : UITableViewController, IViewControllerView
    {
        #region Fields

        private IMvvmViewControllerMediator _mediator;

        #endregion

        #region Constructors

        protected MvvmTableViewController()
        {
        }

        protected MvvmTableViewController(NSCoder coder)
            : base(coder)
        {
        }

        protected MvvmTableViewController(NSObjectFlag t)
            : base(t)
        {
        }

        protected MvvmTableViewController(IntPtr handle)
            : base(handle)
        {
        }

        protected MvvmTableViewController(string nibName, NSBundle bundle)
            : base(nibName, bundle)
        {
        }

        protected MvvmTableViewController(UITableViewStyle withStyle)
            : base(withStyle)
        {
        }

        #endregion

        #region Implementation of IViewControllerView

        /// <summary>
        ///     Gets the current <see cref="IMvvmViewControllerMediator" />.
        /// </summary>
        public virtual IMvvmViewControllerMediator Mediator
        {
            get { return this.GetOrCreateMediator(ref _mediator); }
        }

        #endregion

        #region Overrides of UIViewController

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
    }
}