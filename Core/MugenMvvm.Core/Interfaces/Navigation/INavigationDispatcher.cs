using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcher : IComponentOwner<INavigationDispatcher>, IComponent<IMugenApplication>
    {
        INavigationContext GetNavigationContext(INavigationProvider navigationProvider, string navigationOperationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type = null, IReadOnlyMetadataContext? metadata = null);

        INavigationEntry? TryGetPreviousNavigationEntry(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<INavigationCallback> GetCallbacks(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null);

        Task<bool> OnNavigatingAsync(INavigationContext navigationContext, CancellationToken cancellationToken = default);

        void OnNavigated(INavigationContext navigationContext);

        void OnNavigationFailed(INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationContext navigationContext);
    }
}