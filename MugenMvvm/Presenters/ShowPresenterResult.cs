using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;

namespace MugenMvvm.Presenters
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ShowPresenterResult
    {
        #region Constructors

        public ShowPresenterResult(IPresenterResult result, INavigationCallback? showingCallback, INavigationCallback closeCallback)
        {
            Should.NotBeNull(result, nameof(result));
            Result = result;
            ShowingCallback = showingCallback;
            CloseCallback = closeCallback;
        }

        #endregion

        #region Properties

        public IPresenterResult Result { get; }

        public INavigationCallback? ShowingCallback { get; }

        public INavigationCallback CloseCallback { get; }

        #endregion
    }
}