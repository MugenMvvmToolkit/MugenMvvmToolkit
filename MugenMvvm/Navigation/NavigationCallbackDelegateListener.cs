using System;
using System.Threading;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationCallbackDelegateListener : INavigationCallbackListener
    {
        #region Fields

        public static readonly NavigationCallbackDelegateListener DisposeTargetCallback = new NavigationCallbackDelegateListener((context, exception, arg3) => (context.Target as IDisposable)?.Dispose());

        #endregion

        #region Constructors

        public NavigationCallbackDelegateListener(Action<INavigationContext, Exception?, CancellationToken?> callback)
        {
            Should.NotBeNull(callback, nameof(callback));
            Callback = callback;
        }

        #endregion

        #region Properties

        public Action<INavigationContext, Exception?, CancellationToken?> Callback { get; }

        #endregion

        #region Implementation of interfaces

        public void OnCompleted(INavigationContext navigationContext) => Callback.Invoke(navigationContext, null, null);

        public void OnError(INavigationContext navigationContext, Exception exception) => Callback.Invoke(navigationContext, exception, null);

        public void OnCanceled(INavigationContext navigationContext, CancellationToken cancellationToken) => Callback.Invoke(navigationContext, null, cancellationToken);

        #endregion
    }
}