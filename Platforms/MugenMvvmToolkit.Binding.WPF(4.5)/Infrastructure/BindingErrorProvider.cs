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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.MarkupExtensions;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using MugenMvvmToolkit.Binding.Models;
#endif

namespace MugenMvvmToolkit.Binding.Infrastructure
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
            var depObj = target as DependencyObject;
            if (depObj == null)
            {
                base.SetErrors(target, errors, context);
                return;
            }
            View.SetErrors(depObj, errors);
#if NETFX_CORE || WINDOWSCOMMON
            var control = depObj as Control;
            if (control != null)
                VisualStateManager.GoToState(control, errors.Count == 0 ? "Valid" : "Invalid", true);
#else
            var element = depObj as FrameworkElement;
            if (element != null)
                ValidationBinder.SetErrors(element, errors);
#endif
        }

        #endregion
    }
}
