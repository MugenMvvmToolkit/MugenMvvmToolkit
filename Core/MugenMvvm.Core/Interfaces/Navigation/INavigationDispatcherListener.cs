using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcherListener : IListener
    {
        Task<bool> OnNavigatingAsync(INavigationDispatcher navigationDispatcher, INavigationContext context);

        void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext context);

        void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext context, Exception exception);

        void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext context);

        void OnNavigatingCanceled(INavigationDispatcher navigationDispatcher, INavigationContext context);
    }
}