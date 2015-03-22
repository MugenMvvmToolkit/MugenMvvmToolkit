using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Primitives;

namespace MugenMvvmToolkit.Models
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