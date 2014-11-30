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

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the class that provides a user interface for indicating that a control on a form has an error associated
    ///     with it using the <see cref="ErrorProvider" /> component.
    /// </summary>
    public class BindingErrorProvider : BindingErrorProviderBase
    {
        #region Fields

        private const string ErrorProviderName = "#_b_e_p";

        #endregion

        #region Overrides of BindingErrorProviderBase

        /// <summary>
        ///     Sets errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="errors">The collection of errors</param>
        /// <param name="context">The specified context, if any.</param>
        protected override sealed void SetErrors(object target, IList<object> errors, IDataContext context)
        {
            base.SetErrors(target, errors, context);
            var control = target as Control;
            if (control == null)
                return;
            Control rootControl = PlatformExtensions.GetRootControl(control);
            if (rootControl == null)
                return;
            ErrorProvider errorProvider = GetErrorProvider(rootControl);
            if (errorProvider == null)
                return;

            var oldProvider = ServiceProvider
                .AttachedValueProvider
                .GetValue<ErrorProvider>(target, ErrorProviderName, false);
            if (!ReferenceEquals(oldProvider, errorProvider))
            {
                if (oldProvider != null)
                    oldProvider.SetError(control, null);
                ServiceProvider.AttachedValueProvider.SetValue(control, ErrorProviderName, errorProvider);
            }
            SetErrors(control, errorProvider, errors, context);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sets errors for control.
        /// </summary>
        protected virtual void SetErrors([NotNull] Control target, [NotNull] ErrorProvider errorProvider,
            [NotNull] IList<object> errors, [NotNull] IDataContext context)
        {
            errorProvider.SetError(target, errors.Count == 0 ? null : string.Join(Environment.NewLine, errors));
        }

        /// <summary>
        ///     Gets an instance of <see cref="ErrorProvider" /> for the specified <see cref="Control" />.
        /// </summary>
        [CanBeNull]
        protected virtual ErrorProvider GetErrorProvider(Control rootControl)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(rootControl, ErrorProviderName, (control, o) => new ErrorProvider { ContainerControl = control.GetContainerControl() as ContainerControl }, null);
        }

        #endregion
    }
}