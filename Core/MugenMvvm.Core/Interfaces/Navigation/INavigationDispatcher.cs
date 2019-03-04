using System;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcher : IHasListeners<INavigationDispatcherListener>
    {
        INavigationContextFactory ContextFactory { get; }

        INavigationDispatcherJournal NavigationJournal { get; }

        INavigatingResult OnNavigating(INavigationContext navigationContext);

        void OnNavigated(INavigationContext navigationContext);

        void OnNavigationFailed(INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationContext navigationContext);
    }
}