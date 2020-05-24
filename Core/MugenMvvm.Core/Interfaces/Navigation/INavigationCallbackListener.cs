using System;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallbackListener
    {
        void OnCompleted(IReadOnlyMetadataContext metadata);

        void OnError(Exception exception, IReadOnlyMetadataContext metadata);

        void OnCanceled(CancellationToken cancellationToken, IReadOnlyMetadataContext metadata);
    }
}