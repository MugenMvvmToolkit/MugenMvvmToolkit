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
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;

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
            var textView = target as TextView;
            if (textView != null)
                textView.Error = errors.FirstOrDefault().ToStringSafe();
            base.SetErrors(target, errors, context);
        }

        #endregion
    }
}