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

        public IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type = null, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<INavigationEntryProviderComponent>(metadata).TryGetNavigationEntries(type, metadata) ?? Default.EmptyArray<INavigationEntry>();
        }

        public Task<bool> OnNavigatingAsync(INavigationContext navigationContext, CancellationToken cancellationToken = default)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            return GetComponents<INavigationDispatcherNavigatingListener>(navigationContext.GetMetadataOrDefault()).OnNavigatingAsync(this, navigationContext, cancellationToken);
        }

        public void OnNavigated(INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            GetComponents<INavigationDispatcherNavigatedListener>(navigationContext.GetMetadataOrDefault()).OnNavigated(this, navigationContext);
        }

        public void OnNavigationFailed(INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            Should.NotBeNull(exception, nameof(exception));
            GetComponents<INavigationDispatcherErrorListener>(navigationContext.GetMetadataOrDefault()).OnNavigationFailed(this, navigationContext, exception);
        }

        public void OnNavigationCanceled(INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            GetComponents<INavigationDispatcherErrorListener>(navigationContext.GetMetadataOrDefault()).OnNavigationCanceled(this, navigationContext);
        }

        #endregion
    }
}