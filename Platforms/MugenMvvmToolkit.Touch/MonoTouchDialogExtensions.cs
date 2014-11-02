using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MonoTouch.Dialog;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.MonoTouch.Dialog
{
    public static class MonoTouchDialogExtensions
    {
        #region Methods

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

        public static void SetParent([NotNull] this Element element, object parent)
        {
            Should.NotBeNull(element, "element");
            if (parent == null)
                BindingExtensions.AttachedParentMember.SetValue(element, BindingExtensions.NullValue);
            else
                BindingExtensions.AttachedParentMember.SetValue(element, parent);
        }

        #endregion
    }
}