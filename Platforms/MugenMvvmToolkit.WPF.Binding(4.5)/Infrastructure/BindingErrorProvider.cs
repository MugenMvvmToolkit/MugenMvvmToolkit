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
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
#if WPF
using System.Windows;
using MugenMvvmToolkit.WPF.Binding.Models;
using MugenMvvmToolkit.WPF.MarkupExtensions;

namespace MugenMvvmToolkit.WPF.Binding.Infrastructure
#elif SILVERLIGHT
using System.Windows;
using MugenMvvmToolkit.Silverlight.Binding.Models;
using MugenMvvmToolkit.Silverlight.MarkupExtensions;

namespace MugenMvvmToolkit.Silverlight.Binding.Infrastructure
#elif XAMARIN_FORMS && WINDOWSCOMMON
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MugenMvvmToolkit.Xamarin.Forms.WinRT.Binding.Infrastructure
#elif WINDOWSCOMMON
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MugenMvvmToolkit.WinRT.Binding.Infrastructure
#elif WINDOWS_PHONE
using System.Windows;
using MugenMvvmToolkit.WinPhone.Binding.Models;
using MugenMvvmToolkit.WinPhone.MarkupExtensions;

namespace MugenMvvmToolkit.WinPhone.Binding.Infrastructure
#endif
{
    public class BindingErrorProvider : BindingErrorProviderBase
    {
        #region Overrides of BindingErrorProviderBase

        protected override void SetErrors(object target, IList<object> errors, IDataContext context)
        {
            var depObj = target as DependencyObject;
            if (depObj == null)
            {
                base.SetErrors(target, errors, context);
                return;
            }
#if WINDOWSCOMMON
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
