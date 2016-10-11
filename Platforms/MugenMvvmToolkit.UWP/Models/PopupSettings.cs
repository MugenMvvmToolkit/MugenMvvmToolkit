#region Copyright

// ****************************************************************************
// <copyright file="PopupSettings.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

namespace MugenMvvmToolkit.UWP.Models
{
    public sealed class PopupSettings
    {
        #region Properties

        public Action<Popup> ShowAction { get; set; }

        public Action<Popup> CloseAction { get; set; }

        public Action<Popup, Size> UpdateSizeAction { get; set; }

        public Action<Popup, Size> UpdatePositionAction { get; set; }

        #endregion
    }
}
