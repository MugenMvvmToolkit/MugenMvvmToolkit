using System;
using System.Threading;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Tests.Navigation
{
    public class TestNavigationCallbackListener : INavigationCallbackListener
    {
        public Action<INavigationContext>? OnCompleted { get; set; }

        public Action<INavigationContext, Exception>? OnError { get; set; }

        public Action<INavigationContext, CancellationToken>? OnCanceled { get; set; }

        void INavigationCallbackListener.OnCompleted(INavigationContext navigationContext) => OnCompleted?.Invoke(navigationContext);

        void INavigationCallbackListener.OnError(INavigationContext navigationContext, Exception exception) => OnError?.Invoke(navigationContext, exception);

        void INavigationCallbackListener.OnCanceled(INavigationContext navigationContext, CancellationToken cancellationToken) =>
            OnCanceled?.Invoke(navigationContext, cancellationToken);
    }
}