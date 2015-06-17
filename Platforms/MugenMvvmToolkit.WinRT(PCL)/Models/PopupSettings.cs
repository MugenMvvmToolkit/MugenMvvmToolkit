#region Copyright

// ****************************************************************************
// <copyright file="PopupSettings.cs">
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
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Primitives;

namespace MugenMvvmToolkit.WinRT.Models
{
    /// <summary>
    ///     Represents the popup settings.
    /// </summary>
    public sealed class PopupSettings
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the show action delegate, if any.
        /// </summary>
        public Action<Popup> ShowAction { get; set; }

        /// <summary>
        ///     Gets or sets the close action delegate, if any.
        /// </summary>
        public Action<Popup> CloseAction { get; set; }

        /// <summary>
        ///     Gets or sets the action delegate that allows to set update position, if any.
        /// </summary>
        public Action<Popup, Size> UpdateSizeAction { get; set; }

        /// <summary>
        ///     Gets or sets the action delegate that allows to set update position, if any.
        /// </summary>
        public Action<Popup, Size> UpdatePositionAction { get; set; }

        #endregion
    }
}