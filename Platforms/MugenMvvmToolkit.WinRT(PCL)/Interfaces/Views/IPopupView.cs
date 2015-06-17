#region Copyright

// ****************************************************************************
// <copyright file="IPopupView.cs">
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
using Windows.UI.Xaml.Controls.Primitives;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.WinRT.Models;

namespace MugenMvvmToolkit.WinRT.Interfaces.Views
{
    /// <summary>
    ///     Represents the interface for popup view.
    /// </summary>
    public interface IPopupView : IView
    {
        /// <summary>
        ///     Initializes the specified popup.
        /// </summary>
        void InitializePopup(Popup popup, PopupSettings settings);
    }
}