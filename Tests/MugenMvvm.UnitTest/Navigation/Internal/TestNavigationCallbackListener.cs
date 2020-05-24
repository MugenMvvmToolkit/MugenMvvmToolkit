using System;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationCallbackListener : INavigationCallbackListener
    {
        #region Properties

        public Action<IReadOnlyMetadataContext>? OnCompleted { get; set; }

        public Action<Exception, IReadOnlyMetadataContext?>? OnError { get; set; }

        public Action<IReadOnlyMetadataContext?, CancellationToken>? OnCanceled { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationCallbackListener.OnCompleted(IReadOnlyMetadataContext metadata)
        {
            OnCompleted?.Invoke(metadata);
        }

        void INavigationCallbackListener.OnError(Exception exception, IReadOnlyMetadataContext? metadata)
        {
            OnError?.Invoke(exception, metadata);
        }

        void INavigationCallbackListener.OnCanceled(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            OnCanceled?.Invoke(metadata, cancellationToken);
        }

        #endregion
    }
}