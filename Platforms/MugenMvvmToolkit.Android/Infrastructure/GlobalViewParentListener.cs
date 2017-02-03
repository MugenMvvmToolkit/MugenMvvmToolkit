#region Copyright

// ****************************************************************************
// <copyright file="GlobalViewParentListener.cs">
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
using Android.Runtime;
using Android.Views;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Binding;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.Infrastructure
{
    public sealed class GlobalViewParentListener : Object, ViewGroup.IOnHierarchyChangeListener
    {
        #region Fields

        public static readonly GlobalViewParentListener Instance;

        #endregion

        #region Constructors

        static GlobalViewParentListener()
        {
            Instance = new GlobalViewParentListener();
        }

        [Preserve(Conditional = true)]
        private GlobalViewParentListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        private GlobalViewParentListener()
        {
        }

        #endregion

        #region Implementation of IOnHierarchyChangeListener

        public void OnChildViewAdded(View parent, View child)
        {
            ParentObserver.Raise(child);
            var viewGroup = child as ViewGroup;
            if (viewGroup != null && !viewGroup.IsDisableHierarchyListener())
                viewGroup.SetOnHierarchyChangeListener(this);
        }

        public void OnChildViewRemoved(View parent, View child)
        {
            ParentObserver.Raise(child);
        }

        #endregion
    }
}