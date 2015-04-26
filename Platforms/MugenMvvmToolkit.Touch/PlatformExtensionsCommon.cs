using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces;
using ObjCRuntime;
using UIKit;

namespace MugenMvvmToolkit
{
    public static partial class PlatformExtensions
    {
        #region Fields

        private static readonly List<WeakReference> OrientationChangeListeners = new List<WeakReference>();
        private static bool _hasOrientationChangeSubscriber;
        private static bool? _isOs7;
        private static bool? _isOs8;

        #endregion

        #region Properties

        public static bool IsOS7
        {
            get
            {
                if (_isOs7 == null)
                    _isOs7 = UIDevice.CurrentDevice.CheckSystemVersion(7, 0);
                return _isOs7.Value;
            }
        }

        public static bool IsOS8
        {
            get
            {
                if (_isOs8 == null)
                    _isOs8 = UIDevice.CurrentDevice.CheckSystemVersion(8, 0);
                return _isOs8.Value;
            }
        }

        #endregion

        #region Methods

        public static void DisposeEx<T>(this T[] items)
            where T : INativeObject
        {
            if (items == null)
                return;
            for (int i = 0; i < items.Length; i++)
                items[i].DisposeEx();
        }

        public static void DisposeEx(this INativeObject nativeObject)
        {
            if (!nativeObject.IsAlive() || TryDispose(nativeObject))
                return;

            var view = nativeObject as UIView;
            if (view == null)
            {
                var disposable = nativeObject as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            else
                DisposeView(view);
        }

        public static void ClearBindingsRecursively([CanBeNull]this UIView view, bool clearDataContext, bool clearAttachedValues)
        {
            if (!view.IsAlive())
                return;
            var subviews = view.Subviews;
            if (subviews != null)
            {
                foreach (var subView in subviews)
                    subView.ClearBindingsRecursively(clearDataContext, clearAttachedValues);
            }
            view.ClearBindings(clearDataContext, clearAttachedValues);
        }

        public static void AddOrientationChangeListener([NotNull] IOrientationChangeListener listener)
        {
            Should.NotBeNull(listener, "listener");
            lock (OrientationChangeListeners)
            {
                if (!_hasOrientationChangeSubscriber)
                {
                    UIApplication.Notifications.ObserveDidChangeStatusBarOrientation(DidChangeStatusBarOrientation);
                    _hasOrientationChangeSubscriber = true;
                }
                OrientationChangeListeners.Add(ToolkitExtensions.GetWeakReference(listener));
            }
        }

        public static void RemoveOrientationChangeListener([NotNull]IOrientationChangeListener listener)
        {
            Should.NotBeNull(listener, "listener");
            lock (OrientationChangeListeners)
            {
                for (int i = 0; i < OrientationChangeListeners.Count; i++)
                {
                    var target = OrientationChangeListeners[i].Target;
                    if (target == null)
                    {
                        OrientationChangeListeners.RemoveAt(i);
                        --i;
                        continue;
                    }
                    if (ReferenceEquals(target, listener))
                    {
                        OrientationChangeListeners.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        internal static bool IsAlive([CanBeNull] this INativeObject item)
        {
            return item != null && item.Handle != IntPtr.Zero;
        }

        internal static bool TryDispose(object item)
        {
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(item.GetType(), "Dispose", false, false);
            if (member == null)
                return false;
            member.GetValue(item, Empty.Array<object>());
            return true;
        }

        private static void DidChangeStatusBarOrientation(object sender, UIStatusBarOrientationChangeEventArgs orientation)
        {
            if (OrientationChangeListeners.Count == 0)
                return;
            var listeners = new List<IOrientationChangeListener>(OrientationChangeListeners.Count);
            lock (OrientationChangeListeners)
            {
                for (int i = 0; i < OrientationChangeListeners.Count; i++)
                {
                    var target = (IOrientationChangeListener)OrientationChangeListeners[i].Target;
                    if (target == null)
                    {
                        OrientationChangeListeners.RemoveAt(i);
                        --i;
                    }
                    else
                        listeners.Add(target);
                }
            }
            for (int index = 0; index < listeners.Count; index++)
                listeners[index].OnOrientationChanged();
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
                if (view.Layer != null)
                    view.Layer.RemoveAllAnimations();

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
                    if (tableView.DataSource != null)
                        tableView.DataSource.DisposeEx();

                    // NOTE: dont dispose .Source or WeakDataSource as it usually throws
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
                    if (tableViewCell.ImageView != null)
                        tableViewCell.ImageView.DisposeEx();
                }
                else if (view is UICollectionView)
                {
                    disposeSubviewOnly = true; // UICollectionViewController will throw if we dispose it before it
                    var collectionView = (UICollectionView)view;
                    if (collectionView.DataSource != null)
                        collectionView.DataSource.DisposeEx();
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
                    if (collViewCell.ContentView != null)
                        collViewCell.ContentView.DisposeEx();
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

        #endregion
    }
}