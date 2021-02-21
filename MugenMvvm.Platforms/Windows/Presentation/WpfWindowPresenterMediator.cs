using System;
using System.ComponentModel;
using System.Windows;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Windows.Presentation
{
    public sealed class WpfWindowPresenterMediator : WindowViewPresenterMediatorBase<Window>
    {
        private readonly EventHandler _activatedHandler;
        private readonly EventHandler _closedHandler;
        private readonly CancelEventHandler _closingHandler;
        private readonly EventHandler _deactivatedHandler;

        public WpfWindowPresenterMediator(IViewManager? viewManager = null) : base(viewManager)
        {
            _activatedHandler = OnActivated;
            _deactivatedHandler = OnDeactivated;
            _closingHandler = OnClosing;
            _closedHandler = OnClosed;
        }

        protected override void Initialize(IViewModelPresenterMediator mediator, Window view, INavigationContext navigationContext)
        {
            view.Activated += _activatedHandler;
            view.Deactivated += _deactivatedHandler;
            view.Closing += _closingHandler;
            view.Closed += _closedHandler;
        }

        protected override void Cleanup(IViewModelPresenterMediator mediator, Window view, INavigationContext navigationContext)
        {
            view.Activated -= _activatedHandler;
            view.Deactivated -= _deactivatedHandler;
            view.Closing -= _closingHandler;
            view.Closed -= _closedHandler;
        }

        protected override void Activate(IViewModelPresenterMediator mediator, Window view, INavigationContext navigationContext) => view.Activate();

        protected override void Show(IViewModelPresenterMediator mediator, Window view, bool nonModal, INavigationContext navigationContext)
        {
            if (nonModal)
                view.Show();
            else
                view.ShowDialog();
        }

        protected override void Close(IViewModelPresenterMediator mediator, Window view, INavigationContext navigationContext) => view.Close();
    }
}