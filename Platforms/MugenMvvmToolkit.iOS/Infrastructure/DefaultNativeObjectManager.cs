#region Copyright

// ****************************************************************************
// <copyright file="DefaultNativeObjectManager.cs">
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

using System;
using System.Collections;
using System.Linq;
using MonoTouch.Dialog;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.iOS.Binding;
using MugenMvvmToolkit.iOS.Interfaces;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.iOS.MonoTouch.Dialog;
using MugenMvvmToolkit.Interfaces.Models;
using ObjCRuntime;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure
{
    public class DefaultNativeObjectManager : INativeObjectManager
    {
        #region Implementation of interfaces

        public void Initialize(object item, IDataContext context)
        {
            var viewController = item as UIViewController;
            if (viewController != null)
                InitializeViewController(viewController);
        }

        public void Dispose(object item, IDataContext context)
        {
            var viewController = item as UIViewController;
            if (viewController != null)
            {
                DisposeViewController(viewController);
                return;
            }
            var element = item as Element;
            if (element != null)
            {
                var enumerable = element as IEnumerable;
                if (enumerable != null)
                {
                    foreach (object el in enumerable)
                        Dispose(el, context);
                }
                element.Dispose();
                return;
            }

            var nativeObject = item as INativeObject;
            if (!nativeObject.IsAlive())
                return;

            var view = nativeObject as UIView;
            if (view == null)
                (nativeObject as IDisposable)?.Dispose();
            else
                DisposeView(view);
        }

        #endregion

        #region Methods

        private static void InitializeViewController(UIViewController viewController)
        {
            viewController.View?.RaiseParentChanged(true);
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
                    TouchToolkitExtensions.SetHasState(controller, false);
                }
            }

            viewController.TryRaiseAttachedEvent(AttachedMembers.Object.Parent);
        }

        private static void DisposeViewController(UIViewController viewController)
        {
            var viewControllerView = viewController as IViewControllerView;
            if (viewControllerView == null || viewControllerView.Mediator.IsViewLoaded)
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

            var tabBarController = viewController as UITabBarController;
            if (tabBarController == null)
            {
                var splitViewController = viewController as UISplitViewController;
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
            viewController.Dispose();
        }

        //http://education-eco.blogspot.ru/2014/08/best-way-to-recursively-dispose-uiviews.html
        //http://stackoverflow.com/questions/25532870/xamarin-ios-memory-leaks-everywhere
        private static void DisposeView(UIView view)
        {
            if (!view.IsAlive())
                return;
            try
            {
                var disposeSubviewOnly = false;
                view.Layer?.RemoveAllAnimations();

                if (view.Superview != null)
                    view.RemoveFromSuperview();

                var subViews = view.Subviews;
                if (subViews != null)
                {
                    for (int i = 0; i < subViews.Length; i++)
                        subViews[i].DisposeEx();
                }

                if (view is UIActivityIndicatorView)
                {
                    var indicatorView = (UIActivityIndicatorView)view;
                    if (indicatorView.IsAnimating)
                        indicatorView.StopAnimating();
                }
                else if (view is UITableView)
                {
                    var tableView = (UITableView)view;
                    tableView.DataSource?.DisposeEx();

                    // NOTE: don't dispose .Source or WeakDataSource as it usually throws
                    tableView.Source = null;
                    tableView.Delegate = null;
                    tableView.DataSource = null;
                    tableView.WeakDelegate = null;
                    tableView.WeakDataSource = null;
                    var cells = tableView.VisibleCells;
                    if (cells != null)
                    {
                        for (int i = 0; i < cells.Length; i++)
                            cells[i].DisposeEx();
                    }
                }
                else if (view is UITableViewCell)
                {
                    disposeSubviewOnly = true;
                    var tableViewCell = view as UITableViewCell;
                    tableViewCell.ImageView?.DisposeEx();
                }
                else if (view is UICollectionView)
                {
                    disposeSubviewOnly = true; // UICollectionViewController will throw if we dispose it before it
                    var collectionView = (UICollectionView)view;
                    collectionView.DataSource?.DisposeEx();
                    collectionView.Source = null;
                    collectionView.Delegate = null;
                    collectionView.DataSource = null;
                    collectionView.WeakDelegate = null;
                    collectionView.WeakDataSource = null;
                    var cells = collectionView.VisibleCells;
                    if (cells != null)
                    {
                        for (int i = 0; i < cells.Length; i++)
                            cells[i].DisposeEx();
                    }
                }
                else if (view is UICollectionViewCell)
                {
                    disposeSubviewOnly = true;
                    var collViewCell = view as UICollectionViewCell;
                    collViewCell.ContentView?.DisposeEx();
                }
                else if (view is UIWebView)
                {
                    var webView = (UIWebView)view;
                    if (webView.IsLoading)
                        webView.StopLoading();
                    webView.LoadHtmlString(string.Empty, null); // clear display
                    webView.Delegate = null;
                    webView.WeakDelegate = null;
                }

                var constraints = view.Constraints;
                if (constraints != null && constraints.Length > 0 && constraints.All(c => c.Handle != IntPtr.Zero))
                {
                    view.RemoveConstraints(constraints);
                    for (int i = 0; i < constraints.Length; i++)
                        constraints[i].DisposeEx();
                }

                if (!disposeSubviewOnly)
                    view.Dispose();
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));

            }
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
            item?.SetBindingMemberValue(AttachedMembers.Object.Parent, parent);
        }

        #endregion
    }
}