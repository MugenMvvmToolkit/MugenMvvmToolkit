using System;
using System.Threading;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationCallbackDelegateListener : INavigationCallbackListener
    {
        public static readonly NavigationCallbackDelegateListener DisposeTargetCallback = new((context, exception, arg3) => (context.Target as IDisposable)?.Dispose(), true);

        public NavigationCallbackDelegateListener(Action<INavigationContext, Exception?, CancellationToken?> callback, bool isSerializable)
        {
            Should.NotBeNull(callback, nameof(callback));
            Callback = callback;
            IsSerializable = isSerializable;
        }

        public Action<INavigationContext, Exception?, CancellationToken?> Callback { get; }

        public bool IsSerializable { get; }

        public void OnCompleted(INavigationContext navigationContext) => Callback.Invoke(navigationContext, null, null);

        public void OnError(INavigationContext navigationContext, Exception exception) => Callback.Invoke(navigationContext, exception, null);

        public void OnCanceled(INavigationContext navigationContext, CancellationToken cancellationToken) => Callback.Invoke(navigationContext, null, cancellationToken);
    }
}