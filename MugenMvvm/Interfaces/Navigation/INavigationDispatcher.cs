using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcher : IComponentOwner<INavigationDispatcher>
    {
        INavigationContext? TryGetNavigationContext(object? target, INavigationProvider navigationProvider, string navigationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null);

        ItemOrIReadOnlyList<INavigationEntry> GetNavigationEntries(IReadOnlyMetadataContext? metadata = null);

        ItemOrIReadOnlyList<INavigationCallback> GetNavigationCallbacks(object request, IReadOnlyMetadataContext? metadata = null);

        void OnNavigating(INavigationContext navigationContext);

        Task<bool> OnNavigatingAsync(INavigationContext navigationContext, CancellationToken cancellationToken = default);

        void OnNavigated(INavigationContext navigationContext);

        void OnNavigationFailed(INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationContext navigationContext, CancellationToken cancellationToken = default);
    }
}