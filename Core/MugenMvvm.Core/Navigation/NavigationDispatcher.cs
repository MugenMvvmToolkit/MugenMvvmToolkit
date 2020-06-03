using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public NavigationDispatcher(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public INavigationContext GetNavigationContext(object? target, INavigationProvider navigationProvider, string navigationId, NavigationType navigationType, NavigationMode navigationMode,
            IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<INavigationContextProviderComponent>(metadata).TryGetNavigationContext(target, navigationProvider, navigationId, navigationType, navigationMode, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        public ItemOrList<INavigationEntry, IReadOnlyList<INavigationEntry>> GetNavigationEntries(IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<INavigationEntryProviderComponent>(metadata).TryGetNavigationEntries(metadata);
        }

        public ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> GetNavigationCallbacks<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<INavigationCallbackManagerComponent>(metadata).TryGetNavigationCallbacks(request, metadata);
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

        public void OnNavigationCanceled(INavigationContext navigationContext, CancellationToken cancellationToken = default)
        {
            GetComponents<INavigationDispatcherErrorListener>(navigationContext.GetMetadataOrDefault()).OnNavigationCanceled(this, navigationContext, cancellationToken);
        }

        #endregion
    }
}