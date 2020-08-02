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
using MugenMvvm.Internal;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationDispatcher : ComponentOwnerBase<INavigationDispatcher>, INavigationDispatcher
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationDispatcher(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public INavigationContext? TryGetNavigationContext(object? target, INavigationProvider navigationProvider, string navigationId, NavigationType navigationType, NavigationMode navigationMode,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<INavigationContextProviderComponent>(metadata).TryGetNavigationContext(this, target, navigationProvider, navigationId, navigationType, navigationMode, metadata);
        }

        public ItemOrList<INavigationEntry, IReadOnlyList<INavigationEntry>> GetNavigationEntries(IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<INavigationEntryProviderComponent>(metadata).TryGetNavigationEntries(this, metadata);
        }

        public ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> GetNavigationCallbacks(object request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<INavigationCallbackManagerComponent>(metadata).TryGetNavigationCallbacks(this, request, metadata);
        }

        public void OnNavigating(INavigationContext navigationContext)
        {
            GetComponents<INavigationDispatcherNavigatingListener>(navigationContext.GetMetadataOrDefault()).OnNavigating(this, navigationContext);
        }

        public Task<bool> OnNavigatingAsync(INavigationContext navigationContext, CancellationToken cancellationToken = default)
        {
            var meta = navigationContext.GetMetadataOrDefault();
            return GetComponents<IConditionNavigationDispatcherComponent>(meta).OnNavigatingAsync(GetComponents<INavigationDispatcherNavigatingListener>(meta), this, navigationContext, cancellationToken);
        }

        public void OnNavigated(INavigationContext navigationContext)
        {
            GetComponents<INavigationDispatcherNavigatedListener>(navigationContext.GetMetadataOrDefault()).OnNavigated(this, navigationContext);
        }

        public void OnNavigationFailed(INavigationContext navigationContext, Exception exception)
        {
            GetComponents<INavigationDispatcherErrorListener>(navigationContext.GetMetadataOrDefault()).OnNavigationFailed(this, navigationContext, exception);
        }

        public void OnNavigationCanceled(INavigationContext navigationContext, CancellationToken cancellationToken = default)
        {
            GetComponents<INavigationDispatcherErrorListener>(navigationContext.GetMetadataOrDefault()).OnNavigationCanceled(this, navigationContext, cancellationToken);
        }

        #endregion
    }
}