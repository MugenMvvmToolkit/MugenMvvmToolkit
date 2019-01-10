using System;
using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigatingResult
    {
        Task<bool> GetResultAsync();

        void CompleteNavigation(Func<INavigationDispatcher, INavigationContext, bool> completeNavigationCallback,
            Action<INavigationDispatcher, INavigationContext, Exception?>? canceledCallback = null);
    }
}