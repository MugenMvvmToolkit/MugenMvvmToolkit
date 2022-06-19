using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Presentation;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Presentation
{
    public class FragmentDialogViewPresenterMediator : ViewPresenterMediatorBase<IDialogFragmentView>
    {
        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IViewManager? _viewManager;

        public FragmentDialogViewPresenterMediator(IThreadDispatcher? threadDispatcher = null, INavigationDispatcher? navigationDispatcher = null, IViewManager? viewManager = null)
            : base(threadDispatcher)
        {
            _navigationDispatcher = navigationDispatcher;
            _viewManager = viewManager;
        }

        public override NavigationType NavigationType => NavigationType.Popup;

        protected override bool IsActivateSupported => false;

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected override Task ActivateAsync(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext,
            CancellationToken cancellationToken) => Task.CompletedTask;

        protected override async Task ShowAsync(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext,
            CancellationToken cancellationToken)
        {
            if (navigationContext.NavigationMode == NavigationMode.New)
            {
                view.Cancelable = !navigationContext.GetOrDefault(NavigationMetadata.Modal);
                IActivityView topView;
                while (true)
                {
                    topView = await NavigationDispatcher.GetTopViewAsync<IActivityView>(null, true, mediator.ViewModel, navigationContext.GetMetadataOrDefault());//todo review
                    if (_viewManager.DefaultIfNull().IsInState(topView, ViewLifecycleState.Appeared) || await StateWatcher.WaitAsync(_viewManager, topView))
                        break;
                }

                FragmentMugenExtensions.Show(view, topView, null!);
            }
        }

        protected override Task CloseAsync(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext,
            CancellationToken cancellationToken)
        {
            view.DismissAllowingStateLoss();
            return Task.CompletedTask;
        }

        private sealed class StateWatcher : TaskCompletionSource<bool>, IViewLifecycleListener
        {
            private readonly object _view;

            private StateWatcher(object view)
            {
                _view = view;
            }

            public static Task<bool> WaitAsync(IViewManager? viewManager, object view)
            {
                var watcher = new StateWatcher(view);
                viewManager.DefaultIfNull().AddComponent(watcher);
                return watcher.Task;
            }

            public void OnLifecycleChanged(IViewManager viewManager, ViewInfo view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                if (!view.IsSameView(_view))
                    return;

                if (lifecycleState.BaseState == ViewLifecycleState.Appeared)
                {
                    viewManager.RemoveComponent(this);
                    TrySetResult(true);
                }
                else if (lifecycleState.BaseState == ViewLifecycleState.Cleared)
                {
                    viewManager.RemoveComponent(this);
                    TrySetResult(false);
                }
            }
        }
    }
}