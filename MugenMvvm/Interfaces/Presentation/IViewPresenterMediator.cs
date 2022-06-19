using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Interfaces.Presentation
{
    public interface IViewPresenterMediator
    {
        NavigationType NavigationType { get; }

        object? TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext);

        Task ActivateAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext, CancellationToken cancellationToken);

        Task ShowAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext, CancellationToken cancellationToken);

        Task CloseAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext, CancellationToken cancellationToken);
    }
}