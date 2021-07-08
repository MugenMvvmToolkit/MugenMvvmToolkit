using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;

namespace MugenMvvm.Tests.Presentation
{
    public class TestViewPresenterMediator : IViewPresenterMediator
    {
        public Func<IViewModelPresenterMediator, object?, INavigationContext, object?>? TryGetViewRequest { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Initialize { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Cleanup { get; set; }

        public Func<IViewModelPresenterMediator, object, INavigationContext, CancellationToken, Task>? Activate { get; set; }

        public Func<IViewModelPresenterMediator, object, INavigationContext, CancellationToken, Task>? Show { get; set; }

        public Func<IViewModelPresenterMediator, object, INavigationContext, CancellationToken, Task>? Close { get; set; }

        public NavigationType NavigationType { get; set; } = NavigationType.Alert;

        object? IViewPresenterMediator.TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext)
            => TryGetViewRequest?.Invoke(mediator, view, navigationContext);

        void IViewPresenterMediator.Initialize(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Initialize?.Invoke(mediator, view, navigationContext);

        void IViewPresenterMediator.Cleanup(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext)
            => Cleanup?.Invoke(mediator, view, navigationContext);

        Task IViewPresenterMediator.ActivateAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext, CancellationToken cancellationToken)
            => Activate?.Invoke(mediator, view, navigationContext, cancellationToken) ?? Task.CompletedTask;

        Task IViewPresenterMediator.ShowAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext, CancellationToken cancellationToken)
            => Show?.Invoke(mediator, view, navigationContext, cancellationToken) ?? Task.CompletedTask;

        Task IViewPresenterMediator.CloseAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext, CancellationToken cancellationToken)
            => Close?.Invoke(mediator, view, navigationContext, cancellationToken) ?? Task.CompletedTask;
    }
}