using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Presentation
{
    public abstract class ViewPresenterMediatorBase<TView> : IViewPresenterMediatorProviderComponent, IViewPresenterMediator, IHasPriority where TView : class
    {
        private readonly IThreadDispatcher? _threadDispatcher;

        protected ViewPresenterMediatorBase(IThreadDispatcher? threadDispatcher)
        {
            _threadDispatcher = threadDispatcher;
        }

        public abstract NavigationType NavigationType { get; }

        public int Priority { get; init; }

        protected virtual ThreadExecutionMode ExecutionMode => ThreadExecutionMode.Main;

        protected virtual bool IsActivateSupported => true;

        protected virtual bool IsShowSupported => true;

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        protected abstract Task ActivateAsync(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext, CancellationToken cancellationToken);

        protected abstract Task ShowAsync(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext, CancellationToken cancellationToken);

        protected abstract Task CloseAsync(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext, CancellationToken cancellationToken);

        protected virtual bool CanPresent(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata) =>
            typeof(TView).IsAssignableFrom(mapping.ViewType);

        protected virtual object? TryGetViewRequest(IViewModelPresenterMediator mediator, TView? view, INavigationContext navigationContext) => null;

        object? IViewPresenterMediator.TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext)
            => TryGetViewRequest(mediator, (TView?) view, navigationContext);

        async Task IViewPresenterMediator.ActivateAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsActivateSupported)
                return;
            await ThreadDispatcher.SwitchToAsync(ExecutionMode);
            cancellationToken.ThrowIfCancellationRequested();
            await ActivateAsync(mediator, (TView) view, navigationContext, cancellationToken).ConfigureAwait(false);
        }

        async Task IViewPresenterMediator.ShowAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsShowSupported)
                return;
            await ThreadDispatcher.SwitchToAsync(ExecutionMode);
            cancellationToken.ThrowIfCancellationRequested();
            await ShowAsync(mediator, (TView) view, navigationContext, cancellationToken).ConfigureAwait(false);
        }

        async Task IViewPresenterMediator.CloseAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            await ThreadDispatcher.SwitchToAsync(ExecutionMode);
            cancellationToken.ThrowIfCancellationRequested();
            await CloseAsync(mediator, (TView) view, navigationContext, cancellationToken).ConfigureAwait(false);
        }

        IViewPresenterMediator? IViewPresenterMediatorProviderComponent.TryGetViewPresenter(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping,
            IReadOnlyMetadataContext? metadata) => CanPresent(presenter, viewModel, mapping, metadata) ? this : null;
    }
}