using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcherListener : IListener
    {
        Task<bool> OnNavigatingAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);

        void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);

        void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);

        void OnNavigatingCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);
    }
}