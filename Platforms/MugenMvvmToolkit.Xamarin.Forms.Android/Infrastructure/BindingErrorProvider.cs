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
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Xamarin.Forms;
using View = Android.Views.View;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the class that provides a user interface for indicating that a control on a form has an error associated
    ///     with it.
    /// </summary>
    public class BindingErrorProvider : BindingErrorProviderBase
    {
        #region Fields

        protected const string NativeViewKey = "##NativeView";

        #endregion

        #region Constructors

        static BindingErrorProvider()
        {
            Forms.ViewInitialized += FormsOnViewInitialized;
        }

        #endregion

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

            var textView = target as TextView;
            if (textView != null)
            {
                object error = errors.FirstOrDefault();
                textView.Error = error == null ? null : error.ToString();
            }
        }

        #endregion

        #region Methods

        [CanBeNull]
        protected virtual View GetNativeView([NotNull] Element element)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<View>(element, NativeViewKey, false);
        }

        private static void FormsOnViewInitialized(object sender, ViewInitializedEventArgs args)
        {
            var view = args.View;
            if (view == null || args.NativeView == null)
                return;
            ServiceProvider.AttachedValueProvider.SetValue(view, NativeViewKey, args.NativeView);
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider == null)
                return;
            var dictionary = GetOrAddErrorsDictionary(view);
            foreach (var item in dictionary)
                errorProvider.SetErrors(view, item.Key, item.Value, DataContext.Empty);
        }

        #endregion
    }
}