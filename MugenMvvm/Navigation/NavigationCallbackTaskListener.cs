using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationCallbackTaskListener : TaskCompletionSource<INavigationContext>, INavigationCallbackListener
    {
        #region Constructors

        public NavigationCallbackTaskListener(bool isSerializable)
        {
            IsSerializable = isSerializable;
        }

        #endregion

        #region Properties

        public bool IsSerializable { get; }

        #endregion

        #region Implementation of interfaces

        public void OnCompleted(INavigationContext navigationContext) => TrySetResult(navigationContext);

        public void OnError(INavigationContext navigationContext, Exception exception) => TrySetException(exception);

        public void OnCanceled(INavigationContext navigationContext, CancellationToken cancellationToken) => TrySetCanceled(cancellationToken);

        #endregion
    }
}