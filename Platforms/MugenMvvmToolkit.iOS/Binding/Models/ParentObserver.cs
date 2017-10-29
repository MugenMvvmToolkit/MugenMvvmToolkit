#region Copyright

// ****************************************************************************
// <copyright file="ParentObserver.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Models
{
    internal sealed class ParentObserver : EventListenerList
    {
        #region Fields

        private const string Key = "!#ParentListener";
        private bool _isAttached;
        private readonly WeakReference _view;
        private WeakReference _parent;

        #endregion

        #region Constructors

        private ParentObserver(UIView view)
        {
            _view = ToolkitServiceProvider.WeakReferenceFactory(view);
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(GetParent(view), Empty.WeakReference, false);
        }

        #endregion

        #region Properties

        [CanBeNull]
        public UIView Source => (UIView)_view.Target;

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

        public static object Get(UIView view)
        {
            var value = GetOrAdd(view);
            var parentObserver = value as ParentObserver;
            if (parentObserver == null)
                return (value as WeakReference)?.Target;
            return parentObserver.Parent;
        }

        public static void Set(UIView view, object parent)
        {
            var value = GetOrAdd(view);
            var parentObserver = value as ParentObserver;
            if (parentObserver != null)
                parentObserver.Parent = parent;
        }

        public static IDisposable AddListener(UIView view, IEventListener listener)
        {
            var value = GetOrAdd(view);
            return (value as ParentObserver)?.AddWithUnsubscriber(listener);
        }

        private static object GetOrAdd([NotNull] UIView view)
        {
            return ToolkitServiceProvider
                .AttachedValueProvider
                .GetOrAdd<UIView, object>(view, Key, (item, o) =>
                {
                    bool? value;
                    item.TryGetBindingMemberValue(AttachedMembersBase.Object.IsFlatTree, out value);
                    if (value == null)
                    {
                        var parent = GetParent(item) as UIView;
                        while (parent != null)
                        {
                            parent.TryGetBindingMemberValue(AttachedMembersBase.Object.IsFlatTree, out value);
                            if (value == null)
                                parent = GetParent(parent) as UIView;
                            else if (value.Value)
                                return ToolkitServiceProvider.WeakReferenceFactory(parent);
                            else
                                break;
                        }
                    }
                    return new ParentObserver(item);
                }, null);
        }

        public static void Raise(UIView view, bool recursively)
        {
            if (view.IsAlive())
            {
                object observer = GetOrAdd(view);
                (observer as ParentObserver)?.Raise(recursively);
            }
        }

        public void Raise(bool recursively = true)
        {
            UIView view = RaiseInternal();
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
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(value, Empty.WeakReference, false);
            Raise(view, EventArgs.Empty);
        }

        private static object GetParent(UIView view)
        {
            if (!view.IsAlive())
                return null;
            var controller = view.NextResponder as UIViewController;
            if (controller != null && controller.View == view)
                return controller;
            return view.Superview;
        }

        private UIView GetSource()
        {
            UIView source = Source;
            if (!source.IsAlive())
            {
                Clear();
                return null;
            }
            return source;
        }

        private UIView RaiseInternal()
        {
            var view = GetSource();
            if (_isAttached || view == null)
                return view;

            object parent = GetParent(view);
            if (ReferenceEquals(parent, _parent.Target))
                return view;
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(parent, Empty.WeakReference, false);
            Raise(view, EventArgs.Empty);
            return view;
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
