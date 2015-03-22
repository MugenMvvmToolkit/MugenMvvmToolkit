using Windows.UI.Xaml.Controls.Primitives;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Views
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