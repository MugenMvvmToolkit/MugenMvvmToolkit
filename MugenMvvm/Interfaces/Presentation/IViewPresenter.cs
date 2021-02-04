using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Interfaces.Presentation
{
    public interface IViewPresenter
    {
        NavigationType NavigationType { get; }

        object? TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext);

        void Initialize(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext);

        void Cleanup(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext);

        Task ActivateAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext);

        Task ShowAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext);

        Task CloseAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext);
    }
}