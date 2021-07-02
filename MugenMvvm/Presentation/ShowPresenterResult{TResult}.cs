using System;
using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;

namespace MugenMvvm.Presentation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ShowPresenterResult<TResult> : IEquatable<ShowPresenterResult<TResult>>
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

        public bool Equals(ShowPresenterResult<TResult> other) =>
            CloseCallback.Equals(other.CloseCallback) && Result.Equals(other.Result) && Equals(ShowingCallback, other.ShowingCallback);

        public override bool Equals(object? obj) => obj is ShowPresenterResult<TResult> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(CloseCallback, Result, ShowingCallback);
    }
}