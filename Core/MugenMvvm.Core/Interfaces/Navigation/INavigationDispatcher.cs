using System;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    //todo add create context!!!
    public interface INavigationDispatcher : IHasListeners<INavigationDispatcherListener>
    {
        INavigationDispatcherJournal NavigationJournal { get; }

        INavigatingResult OnNavigating(INavigationContext navigationContext);

        void OnNavigated(INavigationContext navigationContext);

        void OnNavigationFailed(INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationContext navigationContext);
    }
}