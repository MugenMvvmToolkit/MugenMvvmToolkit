﻿using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
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
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        protected ViewPresenterBase(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public abstract NavigationType NavigationType { get; }

        public int Priority { get; set; }

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        #endregion

        #region Implementation of interfaces

        Task IViewPresenter.WaitBeforeShowAsync(IViewModelPresenterMediator mediator, object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => WaitBeforeShowAsync(mediator, (TView?)view, cancellationToken, metadata);

        public virtual Task WaitBeforeCloseAsync(IViewModelPresenterMediator mediator, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => Task.CompletedTask;

        object? IViewPresenter.TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext)
            => TryGetViewRequest(mediator, (TView?)view, navigationContext);

        void IViewPresenter.Initialize(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => Initialize(mediator, (TView)view, navigationContext);

        void IViewPresenter.Cleanup(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => Cleanup(mediator, (TView)view, navigationContext);

        void IViewPresenter.Activate(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => Activate(mediator, (TView)view, navigationContext);

        void IViewPresenter.Show(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => Show(mediator, (TView)view, navigationContext);

        void IViewPresenter.Close(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => Close(mediator, (TView)view, navigationContext);

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

        protected virtual Task WaitBeforeShowAsync(IViewModelPresenterMediator mediator, TView? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => Task.CompletedTask;

        protected abstract void Activate(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext);

        protected abstract void Show(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext);

        protected abstract void Close(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext);

        #endregion
    }
}