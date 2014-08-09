#region Copyright
// ****************************************************************************
// <copyright file="IBindableApplicationBar.cs">
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
using System.Collections;
using System.Windows;
using Microsoft.Phone.Shell;
using MugenMvvmToolkit.Controls;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents a bindable Application Bar in Windows Phone applications.
    /// </summary>
    public interface IBindableApplicationBar : IApplicationBar
    {
        /// <summary>
        ///     Gets or sets the data context for a <see cref="IBindableApplicationBarItem" /> when it participates in data
        ///     binding.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        ///     Gets or sets the ButtonTemplate property.
        ///     This dependency property indicates the template for a button items that is used together with the
        ///     <see cref="ButtonItemsSource" /> collection to create the application bar buttons.
        /// </summary>
        DataTemplate ButtonItemTemplate { get; set; }

        /// <summary>
        ///     Gets or sets the MenuItemTemplate property.
        ///     This dependency property indicates the template for a <see cref="BindableApplicationBarMenuItem" /> that is used
        ///     together with the <see cref="MenuItemsSource" /> collection to create the application bar MenuItems.
        /// </summary>
        DataTemplate MenuItemTemplate { get; set; }

        /// <summary>
        ///     Gets the original application bar.
        /// </summary>
        IApplicationBar OriginalApplicationBar { get; }

        /// <summary>
        ///     Gets or sets the list of the buttons that appear on the Application Bar.
        /// </summary>
        IEnumerable ButtonItemsSource { get; set; }

        /// <summary>
        ///     Gets or sets the list of the menu items that appear on the Application Bar.
        /// </summary>
        IEnumerable MenuItemsSource { get; set; }

        /// <summary>
        ///     Attaches to the specified target.
        /// </summary>
        void Attach(object target);

        /// <summary>
        ///     Detaches this instance from its associated object.
        /// </summary>
        void Detach();
    }
}