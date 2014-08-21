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
using MugenMvvmToolkit.Binding.Interfaces;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using MugenMvvmToolkit.MarkupExtensions;
#else
using System.Windows;
using MugenMvvmToolkit.Binding.Models;
#endif

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    internal class BindingErrorProvider : IBindingErrorProvider
    {
        #region Implementation of IBindingErrorProvider

        /// <summary>
        ///     Sets errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors(object target, IList<object> errors)
        {
            var validatableControl = target as FrameworkElement;
            if (validatableControl == null || PlatformDataBindingModule.DisableValidationMember.GetValue(validatableControl, null))
                return;

#if NETFX_CORE || WINDOWSCOMMON
            var items = ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(validatableControl, "@$@errors_int", (element, o) =>
                {
                    var list = new ObservableCollection<object>();
                    View.SetErrors(element, new ReadOnlyObservableCollection<object>(list));
                    return list;
                }, null);
            var control = validatableControl as Control;
            if (errors.Count == 0)
            {
                View.SetHasErrors(validatableControl, false);
                items.Clear();
                if (control != null)
                    VisualStateManager.GoToState(control, "Valid", true);
            }
            else
            {
                items.Clear();
                items.AddRange(errors);
                View.SetHasErrors(validatableControl, true);
                if (control != null)
                    VisualStateManager.GoToState(control, "Invalid", true);
            }
#else
            var binder = (ValidationBinder)ValidationBinder.GetErrorContainer(validatableControl);
            if (binder == null)
            {
                if (errors.Count == 0)
                    return;
                binder = new ValidationBinder();
                ValidationBinder.SetErrorContainer(validatableControl, binder);

                var binding = new System.Windows.Data.Binding(ValidationBinder.PropertyName)
                {
#if WPF && NET4
                    ValidatesOnDataErrors = true,
#else
                    ValidatesOnDataErrors = false,
                    ValidatesOnNotifyDataErrors = true,
#endif
                    Mode = System.Windows.Data.BindingMode.OneWay,
                    Source = binder,
                    ValidatesOnExceptions = false,
                    NotifyOnValidationError = false,
#if WPF
                    NotifyOnSourceUpdated = false,
                    NotifyOnTargetUpdated = false
#endif
                };
                validatableControl.SetBinding(ValidationBinder.ErrorContainerProperty, binding);
            }
            binder.SetErrors(errors);
#endif
        }

        #endregion
    }
}
