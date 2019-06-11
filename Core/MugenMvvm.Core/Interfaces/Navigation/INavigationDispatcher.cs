using System;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcher : IHasListeners<INavigationDispatcherListener>
    {
        INavigationContextProvider ContextProvider { get; set; }

        INavigationDispatcherJournal NavigationJournal { get; set; }

        INavigatingResult OnNavigating(INavigationContext navigationContext);

        void OnNavigated(INavigationContext navigationContext);

        void OnNavigationFailed(INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationContext navigationContext);
    }
}