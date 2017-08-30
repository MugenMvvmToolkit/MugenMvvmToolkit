#region Copyright

// ****************************************************************************
// <copyright file="MonoTouchDialogExtensions.cs">
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

using System.Collections;
using JetBrains.Annotations;
using MonoTouch.Dialog;
using MugenMvvmToolkit.Binding;
using UIKit;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.iOS.MonoTouch.Dialog
{
    public static class MonoTouchDialogExtensions
    {
        #region Methods

        public static void Reload(this Element element, UITableViewRowAnimation animation = UITableViewRowAnimation.None)
        {
            if (element.GetContainerTableView() != null)
                element.GetImmediateRootElement()?.Reload(element, animation);
        }

        public static void RaiseParentChanged([NotNull] this Element element)
        {
            Should.NotBeNull(element, nameof(element));
            element.TryRaiseAttachedEvent(AttachedMembersBase.Object.Parent);
        }

        public static void DisposeEx(this Element element)
        {
            TouchToolkitExtensions.NativeObjectManager?.Dispose(element, null);
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

        #endregion
    }
}
