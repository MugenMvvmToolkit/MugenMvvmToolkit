using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcher : IHasListeners<INavigationDispatcherListener>
    {
        IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext metadata);

        INavigatingResult OnNavigating(INavigationContext context);

        void OnNavigated(INavigationContext context);

        void OnNavigationFailed(INavigationContext context, Exception exception);

        void OnNavigationCanceled(INavigationContext context);
    }
}