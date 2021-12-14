using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;

namespace MugenMvvm.Presentation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ShowPresenterResult<TResult> : IEquatable<ShowPresenterResult<TResult>>
    {
        public readonly INavigationCallback CloseCallback;
        public readonly IPresenterResult Result;
        public readonly INavigationCallback? ShowCallback;

        public ShowPresenterResult(IPresenterResult result, INavigationCallback? showCallback, INavigationCallback closeCallback)
        {
            Should.NotBeNull(result, nameof(result));
            Result = result;
            ShowCallback = showCallback;
            CloseCallback = closeCallback;
        }

        public ValueTask<INavigationContext?> WaitShowAsync(CancellationToken cancellationToken = default) => WaitShowAsync(false, cancellationToken);

        public ValueTask<INavigationContext?> WaitShowAsync(bool isSerializable, CancellationToken cancellationToken = default)
        {
            if (ShowCallback == null)
                return default;
            return ShowCallback.AsTask(isSerializable, cancellationToken)!;
        }

        public bool Equals(ShowPresenterResult<TResult> other) =>
            CloseCallback.Equals(other.CloseCallback) && Result.Equals(other.Result) && Equals(ShowCallback, other.ShowCallback);

        public override bool Equals(object? obj) => obj is ShowPresenterResult<TResult> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(CloseCallback, Result, ShowCallback);
    }
}