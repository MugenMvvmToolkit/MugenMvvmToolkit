using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestViewPresenter : IViewPresenter
    {
        #region Properties

        public NavigationType NavigationType { get; set; } = NavigationType.Alert;

        public Func<IViewModelPresenterMediator, object?, CancellationToken, IReadOnlyMetadataContext?, Task>? WaitBeforeShowAsync { get; set; }

        public Func<IViewModelPresenterMediator, CancellationToken, IReadOnlyMetadataContext?, Task>? WaitBeforeCloseAsync { get; set; }

        public Func<IViewModelPresenterMediator, object?, INavigationContext, object?>? TryGetViewRequest { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Initialize { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Cleanup { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Activate { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Show { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Close { get; set; }

        #endregion

        #region Implementation of interfaces

        Task IViewPresenter.WaitBeforeShowAsync(IViewModelPresenterMediator mediator, object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => WaitBeforeShowAsync?.Invoke(mediator, view, cancellationToken, metadata) ?? Task.CompletedTask;

        Task IViewPresenter.WaitBeforeCloseAsync(IViewModelPresenterMediator mediator, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => WaitBeforeCloseAsync?.Invoke(mediator, cancellationToken, metadata) ?? Task.CompletedTask;

        object? IViewPresenter.TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext)
            => TryGetViewRequest?.Invoke(mediator, view, navigationContext);

        void IViewPresenter.Initialize(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Initialize?.Invoke(mediator, view, navigationContext);

        void IViewPresenter.Cleanup(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Cleanup?.Invoke(mediator, view, navigationContext);

        void IViewPresenter.Activate(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Activate?.Invoke(mediator, view, navigationContext);

        void IViewPresenter.Show(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Show?.Invoke(mediator, view, navigationContext);

        void IViewPresenter.Close(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Close?.Invoke(mediator, view, navigationContext);

        #endregion
    }
}