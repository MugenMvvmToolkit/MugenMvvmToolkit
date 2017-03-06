#region Copyright

// ****************************************************************************
// <copyright file="BindingErrorProvider.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

namespace MugenMvvmToolkit.WPF.Binding.Infrastructure
{
    public class WpfBindingErrorProvider : BindingErrorProviderBase
    {
#elif XAMARIN_FORMS && WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MugenMvvmToolkit.Xamarin.Forms.UWP.Binding.Infrastructure
{
    public class XamarinFormsUwpBindingErrorProvider : BindingErrorProviderBase
    {
#elif WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MugenMvvmToolkit.UWP.Binding.Infrastructure
{
    public class UwpBindingErrorProvider : BindingErrorProviderBase
    {
#elif NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MugenMvvmToolkit.Xamarin.Forms.WinRT.Binding.Infrastructure
{
    public class XamarinFormsWinRTBindingErrorProvider : BindingErrorProviderBase
    {
#endif

        #region Overrides of BindingErrorProviderBase

        protected override void SetErrors(object target, IList<object> errors, IDataContext context)
        {
#if WINDOWS_UWP || NETFX_CORE
            var control = target as Control;
            if (control != null)
                VisualStateManager.GoToState(control, errors.Count == 0 ? "Valid" : "Invalid", true);
#else
            var element = target as FrameworkElement;
            if (element != null)
                ValidationBinder.SetErrors(element, errors);
#endif
        }

        #endregion
    }
}
