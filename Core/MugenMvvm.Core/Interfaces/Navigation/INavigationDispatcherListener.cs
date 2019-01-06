using System;
using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcherListener
    {
        Task<bool> OnNavigatingAsync(INavigationContext context);

        void OnNavigated(INavigationContext context);

        void OnNavigationFailed(INavigationContext context, Exception exception);

        void OnNavigationCanceled(INavigationContext context);

        void OnNavigatingCanceled(INavigationContext context);
    }
}