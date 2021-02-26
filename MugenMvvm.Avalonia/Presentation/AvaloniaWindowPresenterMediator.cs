using System;
using System.ComponentModel;
using Avalonia.Controls;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Presentation;

namespace MugenMvvm.Avalonia.Presentation
{
    public sealed class AvaloniaWindowPresenterMediator : DialogViewPresenterMediatorBase<Window>
    {
        private readonly EventHandler _activatedHandler;
        private readonly EventHandler _closedHandler;
        private readonly EventHandler<CancelEventArgs> _closingHandler;
        private readonly EventHandler _deactivatedHandler;

        public AvaloniaWindowPresenterMediator(IViewManager? viewManager = null, INavigationDispatcher? navigationDispatcher = null) : base(viewManager, navigationDispatcher)
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

        protected override void Show(IViewModelPresenterMediator mediator, Window view, bool modal, INavigationContext navigationContext)
        {
            var owner = TryGetOwner<Window>(mediator, navigationContext, modal, modal);
            if (modal)
                view.ShowDialog(owner!);
            else
            {
                if (owner == null)
                    view.Show();
                else
                    view.Show(owner);
            }
        }

        protected override void Close(IViewModelPresenterMediator mediator, Window view, INavigationContext navigationContext) => view.Close();
    }
}