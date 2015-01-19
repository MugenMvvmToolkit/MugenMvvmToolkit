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
using System.Collections.Generic;
using JetBrains.Annotations;
using MonoTouch.Dialog;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
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

        public static IList<IDataBinding> SetBindings([NotNull] this Element element,
            string bindingExpression,
            IList<object> sources = null)
        {
            Should.NotBeNull(element, "element");
            return BindingServiceProvider.BindingProvider.CreateBindingsFromString(element, bindingExpression, sources);
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

        public static void ClearBindingsHierarchically([CanBeNull] this Element element, bool clearDataContext, bool clearAttachedValues, bool disposeElement)
        {
            if (element == null)
                return;
            var enumerable = element as IEnumerable;
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                    ClearBindingsHierarchically(item as Element, clearDataContext, clearAttachedValues, disposeElement);
            }
            element.ClearBindings(clearDataContext, clearAttachedValues);
            if (disposeElement)
                element.Dispose();
        }

        public static void ClearBindings([CanBeNull] this Element element, bool clearDataContext, bool clearAttachedValues)
        {
            BindingExtensions.ClearBindings(element, clearDataContext, clearAttachedValues);
        }

        #endregion
    }
}