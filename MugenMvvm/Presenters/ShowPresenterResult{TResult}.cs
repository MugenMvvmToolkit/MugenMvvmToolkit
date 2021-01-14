using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;

namespace MugenMvvm.Presenters
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ShowPresenterResult<TResult>
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