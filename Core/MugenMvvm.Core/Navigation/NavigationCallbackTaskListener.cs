using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationCallbackTaskListener : TaskCompletionSource<IReadOnlyMetadataContext>, INavigationCallbackListener
    {
        #region Implementation of interfaces

        public void OnCompleted(IReadOnlyMetadataContext metadata)
        {
            TrySetResult(metadata);
        }

        public void OnError(Exception exception, IReadOnlyMetadataContext metadata)
        {
            TrySetException(exception);
        }

        public void OnCanceled(CancellationToken cancellationToken, IReadOnlyMetadataContext metadata)
        {
            TrySetCanceled(cancellationToken);
        }

        #endregion
    }
}