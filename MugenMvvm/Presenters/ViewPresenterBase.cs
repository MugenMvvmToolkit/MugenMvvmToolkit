using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Presenters
{
    public abstract class ViewPresenterBase<TView> : IViewPresenterProviderComponent, IViewPresenter, IHasPriority where TView : class
    {
        #region Properties

        public abstract NavigationType NavigationType { get; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        object? IViewPresenter.TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext)
            => TryGetViewRequest(mediator, (TView?) view, navigationContext);

        void IViewPresenter.Initialize(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => Initialize(mediator, (TView) view, navigationContext);

        void IViewPresenter.Cleanup(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => Cleanup(mediator, (TView) view, navigationContext);

        Task IViewPresenter.ActivateAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => ActivateAsync(mediator, (TView) view, navigationContext);

        Task IViewPresenter.ShowAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => ShowAsync(mediator, (TView) view, navigationContext);

        Task IViewPresenter.CloseAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => CloseAsync(mediator, (TView) view, navigationContext);

        IViewPresenter? IViewPresenterProviderComponent.TryGetViewPresenter(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata) =>
            CanPresent(presenter, viewModel, mapping, metadata) ? this : null;

        #endregion

        #region Methods

        protected virtual bool CanPresent(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata) => typeof(TView).IsAssignableFrom(mapping.ViewType);

        protected virtual object? TryGetViewRequest(IViewModelPresenterMediator mediator, TView? view, INavigationContext navigationContext) => null;

        protected virtual void Initialize(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext)
        {
        }

        protected virtual void Cleanup(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext)
        {
        }

        protected abstract Task ActivateAsync(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext);

        protected abstract Task ShowAsync(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext);

        protected abstract Task CloseAsync(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext);

        #endregion
    }
}