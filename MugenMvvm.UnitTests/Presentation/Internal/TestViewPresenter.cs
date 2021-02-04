using System;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Presentation.Internal
{
    public class TestViewPresenter : IViewPresenter
    {
        public Func<IViewModelPresenterMediator, object?, INavigationContext, object?>? TryGetViewRequest { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Initialize { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Cleanup { get; set; }

        public Func<IViewModelPresenterMediator, object, INavigationContext, Task>? Activate { get; set; }

        public Func<IViewModelPresenterMediator, object, INavigationContext, Task>? Show { get; set; }

        public Func<IViewModelPresenterMediator, object, INavigationContext, Task>? Close { get; set; }

        public NavigationType NavigationType { get; set; } = NavigationType.Alert;

        object? IViewPresenter.TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext)
            => TryGetViewRequest?.Invoke(mediator, view, navigationContext);

        void IViewPresenter.Initialize(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Initialize?.Invoke(mediator, view, navigationContext);

        void IViewPresenter.Cleanup(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Cleanup?.Invoke(mediator, view, navigationContext);

        Task IViewPresenter.ActivateAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Activate?.Invoke(mediator, view, navigationContext) ?? Default.CompletedTask;

        Task IViewPresenter.ShowAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Show?.Invoke(mediator, view, navigationContext) ?? Default.CompletedTask;

        Task IViewPresenter.CloseAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Close?.Invoke(mediator, view, navigationContext) ?? Default.CompletedTask;
    }
}