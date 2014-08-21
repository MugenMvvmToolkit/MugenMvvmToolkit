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
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the class that provides a user interface for indicating that a control on a form has an error associated
    ///     with it.
    /// </summary>
    internal class BindingErrorProvider : IBindingErrorProvider
    {
        #region Implementation of IBindingErrorProvider

        /// <summary>
        ///     Sets errors for target.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors(object target, IList<object> errors)
        {
            var textView = target as TextView;
            if (textView != null && !AttachedMembersModule.DisableValidationMember.GetValue(textView, null))
                textView.Error = errors.FirstOrDefault().ToStringSafe();

        }

        #endregion
    }
}