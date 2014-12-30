#region Copyright

// ****************************************************************************
// <copyright file="BindingErrorProvider.cs">
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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Microsoft.Phone.Controls;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the class that provides a user interface for indicating that a control on a form has an error associated
    ///     with it.
    /// </summary>
    public class BindingErrorProvider : BindingErrorProviderBase, IEventListener
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

            //Listen renderer change.
            ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(element, "~~#@rendereListener", (el, o) =>
                {
                    BindingServiceProvider.WeakEventManager.Subscribe(el, "Renderer", (IEventListener)o);
                    return (object)null;
                }, this);
            IVisualElementRenderer renderer = view.GetRenderer();
            if (renderer == null)
                return null;
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

        #region Implementation of IEventListener

        bool IEventListener.IsAlive
        {
            get { return true; }
        }

        bool IEventListener.IsWeak
        {
            get { return true; }
        }

        bool IEventListener.TryHandle(object sender, object message)
        {
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider != null)
            {
                var dictionary = GetOrAddErrorsDictionary(sender);
                foreach (var item in dictionary)
                    errorProvider.SetErrors(sender, item.Key, item.Value, DataContext.Empty);
            }
            return true;
        }

        #endregion
    }
}