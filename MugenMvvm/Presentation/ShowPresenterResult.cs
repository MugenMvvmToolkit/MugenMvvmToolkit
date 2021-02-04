using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;

namespace MugenMvvm.Presentation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ShowPresenterResult
    {
        public readonly INavigationCallback CloseCallback;
        public readonly IPresenterResult Result;
        public readonly INavigationCallback? ShowingCallback;

        public ShowPresenterResult(IPresenterResult result, INavigationCallback? showingCallback, INavigationCallback closeCallback)
        {
            Should.NotBeNull(result, nameof(result));
            Result = result;
            ShowingCallback = showingCallback;
            CloseCallback = closeCallback;
        }
    }
}