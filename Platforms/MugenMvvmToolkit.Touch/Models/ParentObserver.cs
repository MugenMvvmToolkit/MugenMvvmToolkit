#region Copyright
// ****************************************************************************
// <copyright file="ParentObserver.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using JetBrains.Annotations;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding.Infrastructure;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the weak parent observer.
    /// </summary>
    public sealed class ParentObserver : EventListenerList
    {
        #region Fields

        private const string Key = "!#ParentListener";
        private bool _isAttached;
        private readonly WeakReference _view;
        private readonly WeakReference _parent;

        #endregion

        #region Constructors

        private ParentObserver(UIView view)
        {
            _view = ServiceProvider.WeakReferenceFactory(view, true);
            _parent = ServiceProvider.WeakReferenceFactory(GetParent(view), true);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the source element.
        /// </summary>
        [CanBeNull]
        public UIView Source
        {
            get { return (UIView)_view.Target; }
        }

        /// <summary>
        ///     Gets or sets the parent of current element.
        /// </summary>
        [CanBeNull]
        public object Parent
        {
            get { return _parent.Target; }
            set
            {
                _isAttached = true;
                SetParent(value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets or adds an instance of <see cref="ParentObserver" />.
        /// </summary>
        public static ParentObserver GetOrAdd([NotNull] UIView view)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(view, Key, (v, o) => new ParentObserver(v), null);
        }

        /// <summary>
        ///     Raises the parent changed event.
        /// </summary>
        public static void Raise(UIView view, bool recursively)
        {
            if (view != null)
                GetOrAdd(view).Raise(recursively);
        }

        /// <summary>
        ///     Raises the parent changed event.
        /// </summary>
        public void Raise(bool recursively = true)
        {
            UIView view;
            Raise(out  view);
            if (recursively && view != null)
                RaiseSubViews(view);
        }

        private void SetParent(object value)
        {
            UIView view = GetSource();
            if (view == null)
                return;

            if (ReferenceEquals(value, _parent.Target))
                return;
            _parent.Target = value;
            Raise(view, EventArgs.Empty);
        }

        private static object GetParent(UIView view)
        {
            //Disposed
            if (view.IsDisposed())
                return null;
            var controller = view.NextResponder as UIViewController;
            if (controller != null && controller.View == view)
                return controller;
            return view.Superview;
        }

        private UIView GetSource()
        {
            UIView source = Source;
            if (source == null)
                Clear();
            return source;
        }

        private void Raise(out UIView view)
        {
            view = GetSource();
            if (view == null || view.IsDisposed())
            {
                view = null;
                return;
            }

            if (_isAttached)
                return;
            object parent = GetParent(view);
            if (ReferenceEquals(parent, _parent.Target))
                return;
            _parent.Target = parent;
            Raise(view, EventArgs.Empty);
        }

        private static void RaiseSubViews(UIView view)
        {
            var subviews = view.Subviews;
            if (subviews == null)
                return;
            for (int index = 0; index < subviews.Length; index++)
                Raise(subviews[index], true);
        }

        #endregion
    }
}