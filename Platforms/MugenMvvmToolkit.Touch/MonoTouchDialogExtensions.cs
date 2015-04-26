#region Copyright

// ****************************************************************************
// <copyright file="MonoTouchDialogExtensions.cs">
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
using System.Collections;
using JetBrains.Annotations;
using MonoTouch.Dialog;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure;
using UIKit;

// ReSharper disable once CheckNamespace

namespace MugenMvvmToolkit.MonoTouch.Dialog
{
    public static class MonoTouchDialogExtensions
    {
        #region Methods

        public static void Reload(this Element element, UITableViewRowAnimation animation = UITableViewRowAnimation.None)
        {
            if (element.GetContainerTableView() == null)
                return;
            var root = element.GetImmediateRootElement();
            if (root != null)
                root.Reload(element, animation);
        }

        public static void RaiseParentChanged([NotNull] this Element element)
        {
            Should.NotBeNull(element, "element");
            BindingExtensions.AttachedParentMember.Raise(element, EventArgs.Empty);
        }

        public static void SetAttachedParent([NotNull] this Element element, object parent)
        {
            Should.NotBeNull(element, "element");
            if (parent == null)
                BindingExtensions.AttachedParentMember.SetValue(element, BindingExtensions.NullValue);
            else
                BindingExtensions.AttachedParentMember.SetValue(element, parent);
        }

        public static void DisposeEx(this Element element)
        {
            if (element == null || PlatformExtensions.TryDispose(element))
                return;
            var enumerable = element as IEnumerable;
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                    DisposeEx(item as Element);
            }
            element.Dispose();
        }

        public static void ClearBindingsRecursively([CanBeNull] this Element element, bool clearDataContext, bool clearAttachedValues)
        {
            if (element == null)
                return;
            var enumerable = element as IEnumerable;
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                    ClearBindingsRecursively(item as Element, clearDataContext, clearAttachedValues);
            }
            element.ClearBindings(clearDataContext, clearAttachedValues);
        }

        public static object GetDataContext([NotNull] this Element item)
        {
            return ViewManager.GetDataContext(item);
        }

        public static void SetDataContext([NotNull] this Element item, object value)
        {
            ViewManager.SetDataContext(item, value);
        }

        #endregion
    }
}