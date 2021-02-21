using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Presentation;
using MugenMvvm.Requests;

namespace MugenMvvm.Windows.Presentation
{
    public abstract class WindowViewPresenterMediatorBase<T> : ViewPresenterMediatorBase<T> where T : class
    {
        private readonly Dictionary<object, INavigationContext> _contextMap;
        private readonly IViewManager? _viewManager;

        protected WindowViewPresenterMediatorBase(IViewManager? viewManager)
        {
            _contextMap = new Dictionary<object, INavigationContext>(InternalEqualityComparer.Reference);
            _viewManager = viewManager;
        }

        public override NavigationType NavigationType => NavigationType.Window;

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        protected abstract void Activate(IViewModelPresenterMediator mediator, T view, INavigationContext navigationContext);

        protected abstract void Show(IViewModelPresenterMediator mediator, T view, bool nonModal, INavigationContext navigationContext);

        protected abstract void Close(IViewModelPresenterMediator mediator, T view, INavigationContext navigationContext);

        protected virtual void OnClosed(object? sender, EventArgs? e) =>
            ViewManager.OnLifecycleChanged(sender!, ViewLifecycleState.Closed, null, GetNavigationContext(sender).GetMetadataOrDefault());

        protected virtual void OnClosing(object? sender, CancelEventArgs e)
        {
            var request = new CancelableRequest();
            ViewManager.OnLifecycleChanged(sender!, ViewLifecycleState.Closing, request, GetNavigationContext(sender).GetMetadataOrDefault());
            request.Cancel = request.Cancel.GetValueOrDefault(e.Cancel);
        }

        protected virtual void OnDeactivated(object? sender, EventArgs? e)
        {
            var metadata = GetNavigationContext(sender).GetMetadataOrDefault();
            ViewManager.OnLifecycleChanged(sender!, ViewLifecycleState.Disappearing, null, metadata);
            ViewManager.OnLifecycleChanged(sender!, ViewLifecycleState.Disappeared, null, metadata);
        }

        protected virtual void OnActivated(object? sender, EventArgs? e)
        {
            var metadata = GetNavigationContext(sender).GetMetadataOrDefault();
            ViewManager.OnLifecycleChanged(sender!, ViewLifecycleState.Appearing, null, metadata);
            ViewManager.OnLifecycleChanged(sender!, ViewLifecycleState.Appeared, null, metadata);
        }

        protected override Task ActivateAsync(IViewModelPresenterMediator mediator, T view, INavigationContext navigationContext)
        {
            using var ctx = AddContext(view, navigationContext);
            Activate(mediator, view, navigationContext);
            return Task.CompletedTask;
        }

        protected override Task ShowAsync(IViewModelPresenterMediator mediator, T view, INavigationContext navigationContext)
        {
            using var ctx = AddContext(view, navigationContext);
            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Appearing, null, navigationContext.GetMetadataOrDefault());
            Show(mediator, view, navigationContext.GetOrDefault(NavigationMetadata.NonModal), navigationContext);
            return Task.CompletedTask;
        }

        protected override Task CloseAsync(IViewModelPresenterMediator mediator, T view, INavigationContext navigationContext)
        {
            using var ctx = AddContext(view, navigationContext);
            Close(mediator, view, navigationContext);
            return Task.CompletedTask;
        }

        protected ActionToken AddContext(object view, INavigationContext navigationContext)
        {
            _contextMap[view] = navigationContext;
            return new ActionToken((v, m) => ((Dictionary<object, INavigationContext>) m!).Remove(v!), view, _contextMap);
        }

        protected INavigationContext? GetNavigationContext(object? view)
        {
            if (view == null)
                return null;
            _contextMap.TryGetValue(view, out var v);
            return v;
        }
    }
}