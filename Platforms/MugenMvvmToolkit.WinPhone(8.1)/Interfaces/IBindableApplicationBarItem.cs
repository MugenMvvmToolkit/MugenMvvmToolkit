#region Copyright

// ****************************************************************************
// <copyright file="IBindableApplicationBarItem.cs">
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

using System.Windows.Input;
using Microsoft.Phone.Shell;

namespace MugenMvvmToolkit.WinPhone.Interfaces
{
    /// <summary>
    ///     An bindable Application Bar button with an icon.
    /// </summary>
    public interface IBindableApplicationBarItem : IApplicationBarMenuItem
    {
        /// <summary>
        ///     Gets or sets the data context for a <see cref="IBindableApplicationBarItem" /> when it participates in data
        ///     binding.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        ///     Gets the original application bar item.
        /// </summary>
        IApplicationBarMenuItem ApplicationBarItem { get; }

        /// <summary>
        ///     Gets or sets a value that indicates whether the Application Bar Item is visible.
        /// </summary>
        /// <returns>
        ///     true if the Application Bar Item is visible; otherwise, false.
        /// </returns>
        bool IsVisible { get; set; }

        /// <summary>
        ///     Gets or sets the command to invoke when this button is pressed.
        /// </summary>
        /// <returns>
        ///     The command to invoke when this button is pressed. The default is null.
        /// </returns>
        ICommand Command { get; set; }

        /// <summary>
        ///     Gets or sets the parameter to pass to the <see cref="Command" />
        ///     property.
        /// </summary>
        /// <returns>
        ///     The parameter to pass to the <see cref="Command" /> property. The
        ///     default is null.
        /// </returns>
        object CommandParameter { get; set; }

        /// <summary>
        ///     Attaches to the specified <see cref="IBindableApplicationBar" />.
        /// </summary>
        void Attach(IBindableApplicationBar applicationBar, int position);

        /// <summary>
        ///     Detaches this instance from its associated object.
        /// </summary>
        void Detach();
    }
}