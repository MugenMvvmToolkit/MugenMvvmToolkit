using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationCallbackTaskListener : TaskCompletionSource<INavigationContext>, INavigationCallbackListener
    {
        public NavigationCallbackTaskListener(bool isSerializable, CancellationToken cancellationToken)
        {
            IsSerializable = isSerializable;
            CancellationToken = cancellationToken;
            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(o => ((NavigationCallbackTaskListener) o!).OnCanceled(), this);
        }

        public bool IsSerializable { get; }

        public CancellationToken CancellationToken { get; }

        public void OnCompleted(INavigationContext navigationContext) => TrySetResult(navigationContext);

        public void OnError(INavigationContext navigationContext, Exception exception) => TrySetException(exception);

        public void OnCanceled(INavigationContext navigationContext, CancellationToken cancellationToken) => TrySetCanceled(cancellationToken);

        private void OnCanceled() => TrySetCanceled(CancellationToken);
    }
}