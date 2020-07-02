using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
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

        ItemOrList<INavigationEntry, IReadOnlyList<INavigationEntry>> GetNavigationEntries(IReadOnlyMetadataContext? metadata = null);

        ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> GetNavigationCallbacks<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);

        Task<bool> OnNavigatingAsync(INavigationContext navigationContext, CancellationToken cancellationToken = default);

        void OnNavigated(INavigationContext navigationContext);

        void OnNavigationFailed(INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationContext navigationContext, CancellationToken cancellationToken = default);
    }
}