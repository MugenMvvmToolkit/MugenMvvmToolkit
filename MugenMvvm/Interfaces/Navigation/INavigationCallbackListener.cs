using System;
using System.Threading;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallbackListener
    {
        void OnCompleted(INavigationContext navigationContext);

        void OnError(INavigationContext navigationContext, Exception exception);

        void OnCanceled(INavigationContext navigationContext, CancellationToken cancellationToken);
    }
}