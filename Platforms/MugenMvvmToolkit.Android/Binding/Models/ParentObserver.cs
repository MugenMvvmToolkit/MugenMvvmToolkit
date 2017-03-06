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
using MugenMvvmToolkit.Binding.Infrastructure;

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
            _view = ServiceProvider.WeakReferenceFactory(view);
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

        public static ParentObserver GetOrAdd(View view)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(view, Key, (v, o) => new ParentObserver(v), null);
        }

        public static void Raise(View view)
        {
            ParentObserver observer;
            if (ServiceProvider.AttachedValueProvider.TryGetValue(view, Key, out observer))
                observer.Raise();
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
