#region Copyright

// ****************************************************************************
// <copyright file="BindingErrorProvider.cs">
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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Microsoft.Phone.Controls;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the class that provides a user interface for indicating that a control on a form has an error associated
    ///     with it.
    /// </summary>
    public class BindingErrorProvider : BindingErrorProviderBase
    {
        #region Overrides of BindingErrorProviderBase

        /// <summary>
        ///     Sets errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="errors">The collection of errors</param>
        /// <param name="context">The specified context, if any.</param>
        protected override void SetErrors(object target, IList<object> errors, IDataContext context)
        {
            base.SetErrors(target, errors, context);

            var element = target as Element;
            if (element != null)
                target = GetNativeView(element);

            var frameworkElement = target as FrameworkElement;
            if (frameworkElement != null)
                ValidationBinder.SetErrors(frameworkElement, errors);
        }

        #endregion

        #region Methods

        [CanBeNull]
        protected virtual FrameworkElement GetNativeView([NotNull] Element element)
        {
            var view = element as VisualElement;
            if (view == null)
                return null;
            IVisualElementRenderer renderer = view.GetRenderer();
            var entryRenderer = renderer as EntryRenderer;
            if (entryRenderer != null)
            {
                if (entryRenderer.Element.IsPassword)
                    return entryRenderer.Control.Children.OfType<PasswordBox>().FirstOrDefault();
                return entryRenderer.Control.Children.OfType<PhoneTextBox>().FirstOrDefault();
            }

            var member = BindingServiceProvider.MemberProvider.GetBindingMember(renderer.GetType(), "Control", true, false);
            if (member == null || !member.CanRead)
                return null;
            return member.GetValue(renderer, null) as FrameworkElement;
        }

        #endregion
    }
}