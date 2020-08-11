using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IViewPresenter
    {
        NavigationType NavigationType { get; }

        Task WaitBeforeShowAsync(IViewModelPresenterMediator mediator, object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        Task WaitBeforeCloseAsync(IViewModelPresenterMediator mediator, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        object? TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext);

        bool Activate(IViewModelPresenterMediator mediator, object view, INavigationContext context);

        void Show(IViewModelPresenterMediator mediator, object view, INavigationContext context);

        void Close(IViewModelPresenterMediator mediator, object view, INavigationContext context);
    }
}