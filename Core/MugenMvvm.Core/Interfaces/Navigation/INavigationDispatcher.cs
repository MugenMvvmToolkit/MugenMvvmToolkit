using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcher : IComponentOwner<INavigationDispatcher>
    {
        Task<bool> OnNavigatingAsync(INavigationContext navigationContext);

        void OnNavigated(INavigationContext navigationContext);

        void OnNavigationFailed(INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationContext navigationContext);
    }
}