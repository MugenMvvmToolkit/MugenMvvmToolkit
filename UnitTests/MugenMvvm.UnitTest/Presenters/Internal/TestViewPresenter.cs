using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestViewPresenter : IViewPresenter
    {
        #region Properties

        public NavigationType NavigationType { get; set; } = NavigationType.Alert;

        public Func<IViewModelPresenterMediator, object?, INavigationContext, object?>? TryGetViewRequest { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Initialize { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Cleanup { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Activate { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Show { get; set; }

        public Action<IViewModelPresenterMediator, object, INavigationContext>? Close { get; set; }

        #endregion

        #region Implementation of interfaces

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