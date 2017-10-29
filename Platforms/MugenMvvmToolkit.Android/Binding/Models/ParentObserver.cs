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
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Android.Binding.Models
{
    internal sealed class ParentObserver : EventListenerList
    {
        #region Fields

        private const string Key = "!#ParentListener";
        private readonly WeakReference _view;
        private WeakReference _parent;
        private bool _isAttached;

        #endregion

        #region Constructors

        private ParentObserver(View view)
        {
            _view = ToolkitServiceProvider.WeakReferenceFactory(view);
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(GetParent(view), Empty.WeakReference, false);
        }

        #endregion

        #region Properties

        [CanBeNull]
        public View Source => (View)_view.Target;

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

        public static object Get(View view)
        {
            var value = GetOrAdd(view);
            var parentObserver = value as ParentObserver;
            if (parentObserver == null)
                return (value as WeakReference)?.Target;
            return parentObserver.Parent;
        }

        public static void Set(View view, object parent)
        {
            var value = GetOrAdd(view);
            var parentObserver = value as ParentObserver;
            if (parentObserver != null)
                parentObserver.Parent = parent;
        }

        public static IDisposable AddListener(View view, IEventListener listener)
        {
            var value = GetOrAdd(view);
            return (value as ParentObserver)?.AddWithUnsubscriber(listener);
        }

        public static void Raise(View view)
        {
            object observer;
            if (ToolkitServiceProvider.AttachedValueProvider.TryGetValue(view, Key, out observer))
                (observer as ParentObserver)?.Raise();
        }

        public void Raise()
        {
            if (_isAttached)
                return;
            var view = GetSource();
            if (view == null)
                return;
            var parent = GetParent(view);
            if (view.Id == global::Android.Resource.Id.Content || ReferenceEquals(parent, _parent.Target))
                return;
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(parent, Empty.WeakReference, false);
            Raise(view, EventArgs.Empty);
        }

        private static object GetOrAdd(View view)
        {
            return ToolkitServiceProvider
                .AttachedValueProvider
                .GetOrAdd<View, object>(view, Key, (item, o) =>
                {
                    bool? value;
                    item.TryGetBindingMemberValue(AttachedMembersBase.Object.IsFlatTree, out value);
                    if (value == null)
                    {
                        var parent = GetParent(item) as View;
                        while (parent != null)
                        {
                            parent.TryGetBindingMemberValue(AttachedMembersBase.Object.IsFlatTree, out value);
                            if (value == null)
                                parent = GetParent(parent) as View;
                            else if (value.Value)
                            {
                                (view as ViewGroup)?.SetDisableHierarchyListener(true);
                                return ToolkitServiceProvider.WeakReferenceFactory(parent);
                            }
                            else
                                break;
                        }
                    }
                    return new ParentObserver(item);
                }, null);
        }

        private void SetParent(object value)
        {
            var view = GetSource();
            if (view == null)
                return;

            if (ReferenceEquals(value, _parent.Target))
                return;
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(value, Empty.WeakReference, false);
            Raise(view, EventArgs.Empty);
        }

        private View GetSource()
        {
            var source = Source;
            if (!source.IsAlive())
            {
                Clear();
                source = null;
            }
            return source;
        }

        private static object GetParent(View view)
        {
            if (!view.IsAlive())
                return null;
            if (view.Id == global::Android.Resource.Id.Content)
                return view.Context.GetActivity();
            object parent = view.Parent;
            if (parent != null)
                return parent;
            var activity = view.Context.GetActivity();
            if (!activity.IsAlive())
                return null;
            var w = activity.Window;
            if (!w.IsAlive())
                return null;
            var d = w.DecorView;
            if (d.IsAlive() && ReferenceEquals(view, d.RootView))
                return activity;
            return null;
        }

        #endregion
    }
}
