#region Copyright

// ****************************************************************************
// <copyright file="MvvmViewControllerMediator.cs">
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
using JetBrains.Annotations;
using MonoTouch.Dialog;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.MonoTouch.Dialog;
using UIKit;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public class MvvmViewControllerMediator : IMvvmViewControllerMediator
    {
        #region Fields

        private UIViewController _viewController;

        #endregion

        #region Constructors

        public MvvmViewControllerMediator([NotNull] UIViewController viewController)
        {
            Should.NotBeNull(viewController, "viewController");
            _viewController = viewController;
            var viewModel = ViewManager.GetDataContext(viewController) as IViewModel;
            if (viewModel == null || !viewModel.Settings.Metadata.Contains(ViewModelConstants.StateNotNeeded))
                viewController.InititalizeRestorationIdentifier();
            DisposeView = true;
        }

        #endregion

        #region Properties

        public UIViewController ViewController
        {
            get { return _viewController; }
        }

        #endregion

        #region Implementation of IMvvmViewControllerMediator

        public bool DisposeView { get; set; }

        public virtual void ViewWillAppear(Action<bool> baseViewWillAppear, bool animated)
        {
            baseViewWillAppear(animated);
            if (_viewController == null)
                return;
            if (_viewController.View != null)
                ParentObserver.Raise(_viewController.View, true);
            UINavigationItem navigationItem = _viewController.NavigationItem;
            if (navigationItem != null)
            {
                SetParent(navigationItem);
                SetParent(navigationItem.LeftBarButtonItem);
                SetParent(navigationItem.LeftBarButtonItems);
                SetParent(navigationItem.RightBarButtonItem);
                SetParent(navigationItem.RightBarButtonItems);
            }
            SetParent(_viewController.EditButtonItem);
            SetParent(_viewController.ToolbarItems);
            var dialogViewController = _viewController as DialogViewController;
            if (dialogViewController != null)
                SetParent(dialogViewController.Root);
            var viewControllers = ViewController.ChildViewControllers;
            foreach (var controller in viewControllers)
                BindingExtensions.AttachedParentMember.Raise(controller, EventArgs.Empty);

            var tabBarController = ViewController as UITabBarController;
            if (tabBarController != null)
            {
                viewControllers = tabBarController.ViewControllers;
                if (viewControllers != null)
                {
                    foreach (var controller in viewControllers)
                    {
                        BindingExtensions.AttachedParentMember.Raise(controller, EventArgs.Empty);
                        controller.RestorationIdentifier = string.Empty;
                    }
                }
            }
            Raise(ViewWillAppearHandler, animated);
        }

        public virtual void ViewDidAppear(Action<bool> baseViewDidAppear, bool animated)
        {
            baseViewDidAppear(animated);
            Raise(ViewDidAppearHandler, animated);
        }

        public virtual void ViewDidDisappear(Action<bool> baseViewDidDisappear, bool animated)
        {
            baseViewDidDisappear(animated);
            Raise(ViewDidDisappearHandler, animated);
        }

        public virtual void ViewDidLoad(Action baseViewDidLoad)
        {
            baseViewDidLoad();
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
            PlatformExtensions.ApplicationStateManager.DecodeState(_viewController, coder);
            Raise(DecodeRestorableStateHandler, coder);
        }

        public virtual void EncodeRestorableState(Action<NSCoder> baseEncodeRestorableState, NSCoder coder)
        {
            baseEncodeRestorableState(coder);
            PlatformExtensions.ApplicationStateManager.EncodeState(_viewController, coder);
            Raise(EncodeRestorableStateHandler, coder);
        }

        public virtual void Dispose(Action<bool> baseDispose, bool disposing)
        {
            Raise(DisposeHandler);
            if (disposing)
            {
                if (_viewController == null)
                    return;
                var bindingContext = BindingServiceProvider.ContextManager.GetBindingContext(_viewController);
                _viewController.View.ClearBindingsHierarchically(true, true, DisposeView);
                _viewController.ClearBindings(false, false);
                _viewController.EditButtonItem.ClearBindings(true, true);
                _viewController.ToolbarItems.ClearBindings(true, true);
                UINavigationItem navigationItem = _viewController.NavigationItem;
                if (navigationItem != null)
                {
                    navigationItem.ClearBindings(true, true);
                    navigationItem.LeftBarButtonItem.ClearBindings(true, true);
                    navigationItem.LeftBarButtonItems.ClearBindings(true, true);
                    navigationItem.RightBarButtonItem.ClearBindings(true, true);
                    navigationItem.RightBarButtonItems.ClearBindings(true, true);
                }
                var dialogViewController = _viewController as DialogViewController;
                if (dialogViewController != null)
                    dialogViewController.Root.ClearBindingsHierarchically(true, true, DisposeView);
                bindingContext.Value = null;
                ServiceProvider.AttachedValueProvider.Clear(_viewController);
                _viewController = null;
            }
            baseDispose(disposing);
        }

        public virtual event EventHandler ViewDidLoadHandler;

        public virtual event EventHandler<ValueEventArgs<NSCoder>> EncodeRestorableStateHandler;

        public virtual event EventHandler DisposeHandler;

        public virtual event EventHandler<ValueEventArgs<bool>> ViewWillAppearHandler;

        public virtual event EventHandler<ValueEventArgs<bool>> ViewDidAppearHandler;

        public virtual event EventHandler<ValueEventArgs<bool>> ViewDidDisappearHandler;

        public virtual event EventHandler<ValueEventArgs<bool>> ViewWillDisappearHandler;

        public virtual event EventHandler<ValueEventArgs<NSCoder>> DecodeRestorableStateHandler;

        #endregion

        #region Methods

        private void SetParent<T>(T[] items)
        {
            if (items == null)
                return;
            for (int index = 0; index < items.Length; index++)
                SetParent(items[index]);
        }

        private void SetParent(object item)
        {
            if (item != null)
                BindingExtensions.AttachedParentMember.SetValue(item, _viewController);
        }

        private void Raise(EventHandler handler)
        {
            if (handler != null)
                handler(_viewController, EventArgs.Empty);
        }

        private void Raise<T>(EventHandler<ValueEventArgs<T>> handler, T value)
        {
            if (handler != null)
                handler(_viewController, new ValueEventArgs<T>(value));
        }

        #endregion
    }
}