#region Copyright

// ****************************************************************************
// <copyright file="MvvmViewControllerMediator.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using JetBrains.Annotations;
using MonoTouch.Dialog;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Binding;
using MugenMvvmToolkit.iOS.Binding.Models;
using MugenMvvmToolkit.iOS.Interfaces.Mediators;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.iOS.MonoTouch.Dialog;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure.Mediators
{
    public class MvvmViewControllerMediator : IMvvmViewControllerMediator
    {
        #region Fields

        private WeakReference _viewController;
        private bool _isAppeared;
        private bool _canDispose;
        private bool _isDisposeCalled;
        private bool _isViewLoaded;

        #endregion

        #region Constructors

        public MvvmViewControllerMediator([NotNull] UIViewController viewController)
        {
            Should.NotBeNull(viewController, nameof(viewController));
            _viewController = ServiceProvider.WeakReferenceFactory(viewController);
            _canDispose = true;
            var viewModel = viewController.DataContext() as IViewModel;
            if (viewModel == null || !viewModel.Settings.Metadata.Contains(ViewModelConstants.StateNotNeeded))
                viewController.InititalizeRestorationIdentifier();
        }

        #endregion

        #region Properties

        [CanBeNull]
        public UIViewController ViewController
        {
            get
            {
                if (_viewController == null)
                    return null;
                return (UIViewController)_viewController.Target;
            }
        }

        #endregion

        #region Implementation of IMvvmViewControllerMediator

        public bool IsDisappeared => _canDispose;

        public bool IsAppeared => !_canDispose;

        public virtual void ViewWillAppear(Action<bool> baseViewWillAppear, bool animated)
        {
            baseViewWillAppear(animated);

            var viewController = ViewController;
            if (viewController != null && !_isAppeared)
            {
                if (viewController.View != null)
                    ParentObserver.Raise(viewController.View, true);
                UINavigationItem navigationItem = viewController.NavigationItem;
                if (navigationItem != null)
                {
                    SetParent(navigationItem, viewController);
                    SetParent(navigationItem.LeftBarButtonItem, viewController);
                    SetParent(navigationItem.LeftBarButtonItems, viewController);
                    SetParent(navigationItem.RightBarButtonItem, viewController);
                    SetParent(navigationItem.RightBarButtonItems, viewController);
                }
                SetParent(viewController.EditButtonItem, viewController);
                SetParent(viewController.ToolbarItems, viewController);
                var dialogViewController = viewController as DialogViewController;
                if (dialogViewController != null)
                    SetParent(dialogViewController.Root, viewController);
                var viewControllers = viewController.ChildViewControllers;
                foreach (var controller in viewControllers)
                    controller.TryRaiseAttachedEvent(AttachedMembers.Object.Parent);

                var tabBarController = viewController as UITabBarController;
                if (tabBarController == null)
                {
                    var splitViewController = viewController as UISplitViewController;
                    viewControllers = splitViewController == null ? null : splitViewController.ViewControllers;
                }
                else
                    viewControllers = tabBarController.ViewControllers;

                if (viewControllers != null)
                {
                    foreach (var controller in viewControllers)
                    {
                        controller.TryRaiseAttachedEvent(AttachedMembers.Object.Parent);
                        PlatformExtensions.SetHasState(controller, false);
                    }
                }

                viewController.TryRaiseAttachedEvent(AttachedMembers.Object.Parent);
                _isAppeared = true;
            }
            Raise(ViewWillAppearHandler, animated);
        }

        public virtual void ViewDidAppear(Action<bool> baseViewDidAppear, bool animated)
        {
            baseViewDidAppear(animated);
            Raise(ViewDidAppearHandler, animated);
            _canDispose = false;
        }

        public virtual void ViewDidDisappear(Action<bool> baseViewDidDisappear, bool animated)
        {
            baseViewDidDisappear(animated);
            Raise(ViewDidDisappearHandler, animated);
            _canDispose = true;
            TryDispose();
        }

        public virtual void ViewDidLoad(Action baseViewDidLoad)
        {
            baseViewDidLoad();
            _isViewLoaded = true;
            Raise(ViewDidLoadHandler);
        }

        public virtual void ViewWillDisappear(Action<bool> baseViewWillDisappear, bool animated)
        {
            baseViewWillDisappear(animated);
            Raise(ViewWillDisappearHandler, animated);
        }

        public virtual void DecodeRestorableState(Action<NSCoder> baseDecodeRestorableState, NSCoder coder)
        {
            baseDecodeRestorableState(coder);
            var viewController = ViewController;
            if (viewController != null)
                PlatformExtensions.ApplicationStateManager.DecodeState(viewController, coder);
            Raise(DecodeRestorableStateHandler, coder);
        }

        public virtual void EncodeRestorableState(Action<NSCoder> baseEncodeRestorableState, NSCoder coder)
        {
            baseEncodeRestorableState(coder);
            var viewController = ViewController;
            if (viewController != null)
                PlatformExtensions.ApplicationStateManager.EncodeState(viewController, coder);
            Raise(EncodeRestorableStateHandler, coder);
        }

        public virtual void Dispose(Action<bool> baseDispose, bool disposing)
        {
            if (disposing)
            {
                if (!_isDisposeCalled)
                {
                    _isDisposeCalled = true;
                    TryDispose();
                    return;
                }
            }
            baseDispose(disposing);
        }

        public virtual event EventHandler<UIViewController, EventArgs> ViewDidLoadHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<NSCoder>> EncodeRestorableStateHandler;

        public virtual event EventHandler<UIViewController, EventArgs> DisposeHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<bool>> ViewWillAppearHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<bool>> ViewDidAppearHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<bool>> ViewDidDisappearHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<bool>> ViewWillDisappearHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<NSCoder>> DecodeRestorableStateHandler;

        #endregion

        #region Methods

        private void TryDispose()
        {
            var viewController = ViewController;
            if (viewController == null)
                return;
            if (!_canDispose || !_isDisposeCalled)
                return;
            Raise(DisposeHandler);

            if (_isViewLoaded)
            {
                viewController.View.ClearBindingsRecursively(true, true);
                viewController.View.DisposeEx();
                viewController.EditButtonItem.ClearBindings(true, true);
                viewController.EditButtonItem.DisposeEx();
                viewController.ToolbarItems.ClearBindings(true, true);
                viewController.ToolbarItems.DisposeEx();
                UINavigationItem navigationItem = viewController.NavigationItem;
                if (navigationItem != null)
                {
                    navigationItem.LeftBarButtonItem.ClearBindings(true, true);
                    navigationItem.LeftBarButtonItem.DisposeEx();
                    navigationItem.LeftBarButtonItems.ClearBindings(true, true);
                    navigationItem.LeftBarButtonItems.DisposeEx();
                    navigationItem.RightBarButtonItem.ClearBindings(true, true);
                    navigationItem.RightBarButtonItem.DisposeEx();
                    navigationItem.RightBarButtonItems.ClearBindings(true, true);
                    navigationItem.RightBarButtonItems.DisposeEx();
                    navigationItem.ClearBindings(true, true);
                    navigationItem.DisposeEx();
                }

                var dialogViewController = viewController as DialogViewController;
                if (dialogViewController != null)
                {
                    dialogViewController.Root.ClearBindingsRecursively(true, true);
                    dialogViewController.Root.DisposeEx();
                }
            }

            var tabBarController = ViewController as UITabBarController;
            if (tabBarController == null)
            {
                var splitViewController = ViewController as UISplitViewController;
                if (splitViewController != null)
                {
                    splitViewController.ViewControllers.ClearBindings(true, true);
                    splitViewController.ViewControllers.DisposeEx();
                }
            }
            else
            {
                tabBarController.ViewControllers.ClearBindings(true, true);
                tabBarController.ViewControllers.DisposeEx();
            }

            viewController.ChildViewControllers.ClearBindings(true, true);
            viewController.ChildViewControllers.DisposeEx();
            viewController.ClearBindings(true, true);
            viewController.DisposeEx();
            ViewDidLoadHandler = null;
            ViewWillAppearHandler = null;
            ViewDidAppearHandler = null;
            ViewDidDisappearHandler = null;
            ViewWillDisappearHandler = null;
            DecodeRestorableStateHandler = null;
            EncodeRestorableStateHandler = null;
            DisposeHandler = null;
            _viewController = null;
        }

        private static void SetParent<T>(T[] items, UIViewController parent)
        {
            if (items == null)
                return;
            for (int index = 0; index < items.Length; index++)
                SetParent(items[index], parent);
        }

        private static void SetParent(object item, UIViewController parent)
        {
            if (item != null)
                item.SetBindingMemberValue(AttachedMembers.Object.Parent, parent);
        }

        private void Raise(EventHandler<UIViewController, EventArgs> handler)
        {
            var viewController = ViewController;
            if (viewController != null && handler != null)
                handler(viewController, EventArgs.Empty);
        }

        private void Raise<T>(EventHandler<UIViewController, ValueEventArgs<T>> handler, T value)
        {
            var viewController = ViewController;
            if (viewController != null && handler != null)
                handler(viewController, new ValueEventArgs<T>(value));
        }

        #endregion
    }
}
