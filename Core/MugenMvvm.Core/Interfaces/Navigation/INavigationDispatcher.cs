using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcher : IHasListeners<INavigationDispatcherListener>
    {
        IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext metadata);

        Task<bool> OnNavigatingAsync(INavigationContext context);

        void OnNavigated(INavigationContext context);

        void OnNavigationFailed(INavigationContext context, Exception exception);

        void OnNavigationCanceled(INavigationContext context);
    }
}