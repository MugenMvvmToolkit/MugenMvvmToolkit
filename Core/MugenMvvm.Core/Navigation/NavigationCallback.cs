using System;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationCallback : INavigationCallback
    {
        #region Fields

        private readonly TaskCompletionSource<IReadOnlyMetadataContext> _taskCompletionSource;

        #endregion

        #region Constructors

        public NavigationCallback(NavigationCallbackType callbackType, string navigationOperationId, NavigationType navigationType)
        {
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNullOrEmpty(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            CallbackType = callbackType;
            NavigationOperationId = navigationOperationId;
            NavigationType = navigationType;
            _taskCompletionSource = new TaskCompletionSource<IReadOnlyMetadataContext>();
        }

        #endregion

        #region Properties

        public NavigationCallbackType CallbackType { get; }

        public string NavigationOperationId { get; }

        public NavigationType NavigationType { get; }

        #endregion

        #region Implementation of interfaces

        public Task<IReadOnlyMetadataContext> WaitAsync()
        {
            return _taskCompletionSource.Task;
        }

        #endregion

        #region Methods

        public void SetResult(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            _taskCompletionSource?.TrySetResult(metadata);
        }

        public void SetException(Exception exception)
        {
            _taskCompletionSource?.TrySetException(exception);
        }

        public void SetCanceled()
        {
            _taskCompletionSource?.TrySetCanceled();
        }

        #endregion
    }
}