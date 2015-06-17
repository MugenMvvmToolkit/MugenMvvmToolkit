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

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.WinForms.Binding.Infrastructure
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
            ErrorProvider errorProvider = GetErrorProviderInternal(rootControl);
            if (errorProvider == null)
                return;

            var oldProvider = ServiceProvider
                .AttachedValueProvider
                .GetValue<ErrorProvider>(target, ErrorProviderName, false);
            if (!ReferenceEquals(oldProvider, errorProvider))
            {
                if (oldProvider != null)
                {
                    oldProvider.SetError(control, null);
                    TryDispose(oldProvider);
                }
                ServiceProvider.AttachedValueProvider.SetValue(control, ErrorProviderName, errorProvider);
                if (errorProvider.Tag == null)
                    errorProvider.Tag = 1;
                else if (errorProvider.Tag is int)
                    errorProvider.Tag = (int)errorProvider.Tag + 1;
            }
            SetErrors(control, errorProvider, errors, context);
        }

        /// <summary>
        ///     Clears the errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="context">The specified context, if any.</param>
        protected override sealed void ClearErrors(object target, IDataContext context)
        {
            base.SetErrors(target, Empty.Array<object>(), context);
            var control = target as Control;
            if (control == null)
                return;
            var errorProvider = ServiceProvider
                .AttachedValueProvider
                .GetValue<ErrorProvider>(target, ErrorProviderName, false);
            if (errorProvider == null)
                return;
            ServiceProvider.AttachedValueProvider.Clear(control, ErrorProviderName);
            errorProvider.SetError(control, null);
            TryDispose(errorProvider);
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
            return new ErrorProvider { ContainerControl = rootControl.GetContainerControl() as ContainerControl };
        }

        [CanBeNull]
        private ErrorProvider GetErrorProviderInternal(Control rootControl)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(rootControl, ErrorProviderName, (control, o) => ((BindingErrorProvider)o).GetErrorProvider(control), this);
        }

        private static void TryDispose(ErrorProvider errorProvider)
        {
            if (!(errorProvider.Tag is int))
                return;
            var count = (int)errorProvider.Tag - 1;
            if (count == 0)
                errorProvider.Dispose();
            else
                errorProvider.Tag = count;
        }

        #endregion
    }
}