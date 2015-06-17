#region Copyright

// ****************************************************************************
// <copyright file="Enums.cs">
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

using MugenMvvmToolkit.Binding.Interfaces;

#if WPF
namespace MugenMvvmToolkit.WPF.Binding.Models
#elif XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Models
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Binding.Models
#elif NETFX_CORE || WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Binding.Models
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Binding.Models
#endif
{
    /// <summary>
    ///     Describes the direction of the data flow in a binding.
    /// </summary>
    public enum BindingModeCore
    {
        /// <summary>
        ///     Uses the default Mode value of the binding target.
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Causes changes to either the source property or the target property to automatically update the other. This type of
        ///     binding is appropriate for editable forms or other fully-interactive UI scenarios.
        /// </summary>
        TwoWay = 1,

        /// <summary>
        ///     Updates the binding target (target) property when the binding source (source) changes. This type of binding is
        ///     appropriate if the control being bound is implicitly read-only. For instance, you may bind to a source such as a
        ///     stock ticker. Or perhaps your target property has no control interface provided for making changes, such as a
        ///     data-bound background color of a table. If there is no need to monitor the changes of the target property, using
        ///     the System.Windows.Data.BindingMode.OneWay binding mode avoids the overhead of the
        ///     System.Windows.Data.BindingMode.TwoWay binding mode.
        /// </summary>
        OneWay = 2,

        /// <summary>
        ///     Updates the binding target when the application starts or when the data context changes. This type of binding is
        ///     appropriate if you are using data where either a snapshot of the current state is appropriate to use or the data is
        ///     truly static. This type of binding is also useful if you want to initialize your target property with some value
        ///     from a source property and the data context is not known in advance. This is essentially a simpler form of
        ///     System.Windows.Data.BindingMode.OneWay binding that provides better performance in cases where the source value
        ///     does not change.
        /// </summary>
        OneTime = 3,

        /// <summary>
        ///     Updates the source property when the target property changes.
        /// </summary>
        OneWayToSource = 4,

        /// <summary>
        ///     Updates the binding source only when you call the <see cref="IDataBinding.UpdateSource" /> method.
        /// </summary>
        None = 5
    }

    /// <summary>
    ///     Describes the timing of binding source updates.
    /// </summary>
    public enum UpdateSourceTriggerCore
    {
        /// <summary>
        ///     The default UpdateSourceTrigger value of the binding target property.
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Updates the binding source immediately whenever the binding target property changes.
        /// </summary>
        PropertyChanged = 1,

        /// <summary>
        ///     Updates the binding source whenever the binding target element loses focus.
        /// </summary>
        LostFocus = 2,
    }
}