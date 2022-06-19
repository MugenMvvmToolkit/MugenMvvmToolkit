using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;

namespace MugenMvvm.Presentation
{
    public abstract class DialogViewPresenterMediatorBase<T> : ViewPresenterMediatorBase<T> where T : class
    {
        private readonly Dictionary<object, INavigationContext> _contextMap;
        private readonly IViewManager? _viewManager;
        private readonly INavigationDispatcher? _navigationDispatcher;
        private bool _isShowing;

        protected DialogViewPresenterMediatorBase(IThreadDispatcher? threadDispatcher, IViewManager? viewManager, INavigationDispatcher? navigationDispatcher)
            : base(threadDispatcher)
        {
            _contextMap = new Dictionary<object, INavigationContext>(InternalEqualityComparer.Reference);
            _viewManager = viewManager;
            _navigationDispatcher = navigationDispatcher;
        }

        public override NavigationType NavigationType => NavigationType.Window;

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        protected abstract void Activate(IViewModelPresenterMediator mediator, T view, INavigationContext navigationContext);

        protected abstract void Show(IViewModelPresenterMediator mediator, T view, bool modal, INavigationContext navigationContext);

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
            if (_isShowing)
                _isShowing = false;
            else
                ViewManager.OnLifecycleChanged(sender!, ViewLifecycleState.Appearing, null, metadata);
            ViewManager.OnLifecycleChanged(sender!, ViewLifecycleState.Appeared, null, metadata);
        }

        protected override Task ActivateAsync(IViewModelPresenterMediator mediator, T view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            using var ctx = AddContext(view, navigationContext);
            Activate(mediator, view, navigationContext);
            return Task.CompletedTask;
        }

        protected override Task ShowAsync(IViewModelPresenterMediator mediator, T view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            using var ctx = AddContext(view, navigationContext);
            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Appearing, null, navigationContext.GetMetadataOrDefault());
            _isShowing = true;
            Show(mediator, view, navigationContext.GetOrDefault(NavigationMetadata.Modal), navigationContext);
            return Task.CompletedTask;
        }

        protected override Task CloseAsync(IViewModelPresenterMediator mediator, T view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            using var ctx = AddContext(view, navigationContext);
            Close(mediator, view, navigationContext);
            return Task.CompletedTask;
        }

        protected ActionToken AddContext(object view, INavigationContext navigationContext)
        {
            _contextMap[view] = navigationContext;
            return ActionToken.FromDelegate((v, m) => ((Dictionary<object, INavigationContext>) m!).Remove(v!), view, _contextMap);
        }

        protected INavigationContext? GetNavigationContext(object? view)
        {
            if (view == null)
                return null;
            _contextMap.TryGetValue(view, out var v);
            return v;
        }

        protected TOwner? TryGetOwner<TOwner>(IViewModelPresenterMediator mediator, INavigationContext navigationContext, bool useParentViewModelAsOwner, bool includeDefault)
            where TOwner : class
        {
            var owner = TryGetView<TOwner>(navigationContext.GetOrDefault(NavigationMetadata.Owner), navigationContext);
            if (owner == null)
            {
                if (useParentViewModelAsOwner)
                    owner = TryGetViewFromParent<TOwner>(mediator.ViewModel, navigationContext);
                if (owner == null && includeDefault)
                    owner = NavigationDispatcher.TryGetTopView<TOwner>(NavigationType, true, mediator.ViewModel, navigationContext.GetMetadataOrDefault());
            }

            return owner;
        }

        private TView? TryGetViewFromParent<TView>(IViewModelBase? viewModel, INavigationContext navigationContext) where TView : class
        {
            viewModel = viewModel?.GetOrDefault(ViewModelMetadata.ParentViewModel);
            while (viewModel != null)
            {
                var view = TryGetView<TView>(viewModel, navigationContext);
                if (view != null)
                    return view;
                viewModel = viewModel.GetOrDefault(ViewModelMetadata.ParentViewModel);
            }

            return null;
        }

        private TView? TryGetView<TView>(object? target, INavigationContext navigationContext) where TView : class
        {
            if (target == null)
                return null;
            if (target is TView view)
                return view;
            foreach (var v in ViewManager.GetViews(target, navigationContext.GetMetadataOrDefault()))
            {
                if (v.Target is TView r)
                    return r;
            }

            return null;
        }
    }
}