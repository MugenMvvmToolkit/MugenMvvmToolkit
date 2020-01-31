using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationDispatcher : ComponentOwnerBase<INavigationDispatcher>, INavigationDispatcher
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationDispatcher(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public INavigationContext GetNavigationContext(INavigationProvider navigationProvider, string navigationOperationId, NavigationType navigationType, NavigationMode navigationMode,
            IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<INavigationContextProviderComponent>(metadata).TryGetNavigationContext(navigationProvider, navigationOperationId, navigationType, navigationMode, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        public IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type = null, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<INavigationEntryProviderComponent>(metadata).TryGetNavigationEntries(type, metadata) ?? Default.EmptyArray<INavigationEntry>();
        }

        public INavigationEntry? TryGetPreviousNavigationEntry(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<INavigationEntryFinderComponent>(metadata).TryGetPreviousNavigationEntry(navigationEntry, metadata);
        }

        public IReadOnlyList<INavigationCallback> GetCallbacks(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<INavigationCallbackProviderComponent>(metadata).TryGetCallbacks(navigationEntry, metadata) ?? Default.EmptyArray<INavigationCallback>();
        }

        public Task<bool> OnNavigatingAsync(INavigationContext navigationContext, CancellationToken cancellationToken = default)
        {
            return GetComponents<INavigationDispatcherNavigatingListener>(navigationContext.GetMetadataOrDefault()).OnNavigatingAsync(this, navigationContext, cancellationToken);
        }

        public void OnNavigated(INavigationContext navigationContext)
        {
            GetComponents<INavigationDispatcherNavigatedListener>(navigationContext.GetMetadataOrDefault()).OnNavigated(this, navigationContext);
        }

        public void OnNavigationFailed(INavigationContext navigationContext, Exception exception)
        {
            GetComponents<INavigationDispatcherErrorListener>(navigationContext.GetMetadataOrDefault()).OnNavigationFailed(this, navigationContext, exception);
        }

        public void OnNavigationCanceled(INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            GetComponents<INavigationDispatcherErrorListener>(navigationContext.GetMetadataOrDefault()).OnNavigationCanceled(this, navigationContext, cancellationToken);
        }

        #endregion
    }
}