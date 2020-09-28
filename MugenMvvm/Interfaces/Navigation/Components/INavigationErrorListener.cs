using System;
using System.Threading;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationErrorListener : IComponent<INavigationDispatcher>
    {
        void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken);
    }
}