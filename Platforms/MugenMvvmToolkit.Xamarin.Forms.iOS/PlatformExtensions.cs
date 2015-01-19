#region Copyright

// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using ObjCRuntime;
using UIKit;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit
{
    public static partial class PlatformExtensions
    {
        #region Fields

        private static readonly List<WeakReference> OrientationChangeListeners;
        private static bool _hasOrientationChangeSubscriber;
        private static bool? _isOs7;
        private static bool? _isOs8;

        #endregion

        #region Constructors

        static PlatformExtensions()
        {
            OrientationChangeListeners = new List<WeakReference>();
        }

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

        public static void RemoveOrientationChangeListener(IOrientationChangeListener listener)
        {
            Should.NotBeNull(listener, "listener");
            lock (OrientationChangeListeners)
            {
                for (int i = 0; i < OrientationChangeListeners.Count; i++)
                {
                    object target = OrientationChangeListeners[i].Target;
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

        public static void ClearBindingsHierarchically([CanBeNull]this UIView view, bool clearDataContext, bool clearAttachedValues, bool disposeView)
        {
            if (view == null)
                return;
            foreach (var subView in view.Subviews)
                subView.ClearBindingsHierarchically(clearDataContext, clearAttachedValues, disposeView);
            BindingExtensions.ClearBindings(view, clearDataContext, clearAttachedValues);
            if (disposeView)
                view.Dispose();
        }

        internal static bool IsAlive([NotNull] this INativeObject item)
        {
            Should.NotBeNull(item, "item");
            return item.Handle != IntPtr.Zero;
        }

        private static void DidChangeStatusBarOrientation(object sender,
            UIStatusBarOrientationChangeEventArgs orientation)
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

        #endregion
    }
}