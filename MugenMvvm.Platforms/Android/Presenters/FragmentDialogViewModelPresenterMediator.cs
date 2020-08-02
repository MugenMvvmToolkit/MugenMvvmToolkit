using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Requests;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Android.Presenters
{
    public class FragmentDialogViewModelPresenterMediator : ViewModelPresenterMediatorBase<IDialogFragmentView>
    {
        #region Fields

        private bool _addedComponent;
        private FragmentViewDispatcher? _fragmentDispatcher;
        private bool _shouldRaiseOnResume;

        #endregion

        #region Constructors

        public FragmentDialogViewModelPresenterMediator(IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, INavigationDispatcher? navigationDispatcher = null,
            IThreadDispatcher? threadDispatcher = null) : base(viewManager, wrapperManager, navigationDispatcher, threadDispatcher)
        {
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => NavigationType.Window;

        private FragmentViewDispatcher FragmentDispatcher => _fragmentDispatcher ??= new FragmentViewDispatcher(this);

        #endregion

        #region Methods

        protected override Task WaitBeforeShowAsync(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return NavigationDispatcher.WaitNavigationAsync(this, (callback, state) => (callback.NavigationType == state.NavigationType || callback.NavigationType == NavigationType.Page)
                                                                                       && (callback.CallbackType == NavigationCallbackType.Showing || callback.CallbackType == NavigationCallbackType.Closing), metadata);
        }

        protected override Task WaitBeforeCloseAsync(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return NavigationDispatcher.WaitNavigationAsync(this, (callback, state) => callback.NavigationType == NavigationType.Background && callback.CallbackType == NavigationCallbackType.Close, metadata);
        }

        protected override void ShowInternal(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!_addedComponent)
            {
                ViewManager.AddComponent(FragmentDispatcher);
                _addedComponent = true;
            }

            _shouldRaiseOnResume = true;
            base.ShowInternal(view, cancellationToken, metadata);
        }

        protected override void ShowView(IDialogFragmentView view, INavigationContext context)
        {
            if (context.NavigationMode == NavigationMode.New)
            {
                view.Cancelable = context.GetMetadataOrDefault().Get(NavigationMetadata.NonModal);
                FragmentExtensions.Show(view, NavigationDispatcher.GetTopView<IActivityView>()!, null!);
            }
        }

        protected override void InitializeView(IDialogFragmentView view, INavigationContext context)
        {
        }

        protected override bool ActivateView(IDialogFragmentView view, INavigationContext context)
        {
            return true;
        }

        protected override void CloseView(IDialogFragmentView view, INavigationContext context)
        {
            view.DismissAllowingStateLoss();
        }

        protected override void CleanupView(IDialogFragmentView view, INavigationContext context)
        {
        }

        protected virtual void OnResumed(IReadOnlyMetadataContext? metadata)
        {
            if (_shouldRaiseOnResume)
            {
                OnViewShown();
                _shouldRaiseOnResume = false;
            }
        }

        protected virtual void OnClosing(ICancelableRequest cancelableRequest, IReadOnlyMetadataContext? metadata)
        {
            if (cancelableRequest.Cancel)
                return;

            var currentView = CurrentView;
            if (currentView == null)
                return;

            if (cancelableRequest is CancelEventArgs args)
                OnViewClosing(currentView, args);
            else
            {
                var eventArgs = new CancelEventArgs(cancelableRequest.Cancel);
                OnViewClosing(currentView, eventArgs);
                cancelableRequest.Cancel = eventArgs.Cancel;
            }
        }

        protected virtual void OnClosed(IReadOnlyMetadataContext? metadata)
        {
            OnViewClosed();
            ViewManager.RemoveComponent(FragmentDispatcher);
            _addedComponent = false;
        }

        #endregion

        #region Nested types

        private sealed class FragmentViewDispatcher : IViewLifecycleDispatcherComponent
        {
            #region Fields

            private readonly FragmentDialogViewModelPresenterMediator _mediator;

            #endregion

            #region Constructors

            public FragmentViewDispatcher(FragmentDialogViewModelPresenterMediator mediator)
            {
                _mediator = mediator;
            }

            #endregion

            #region Implementation of interfaces

            public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                if (view is IView v)
                    view = v.Target;

                var currentView = _mediator.CurrentView;
                if (currentView == null || !Equals(view, currentView))
                    return;

                if (lifecycleState == AndroidViewLifecycleState.Resumed)
                    _mediator.OnResumed(metadata);
                else if ((lifecycleState == AndroidViewLifecycleState.Dismissing || lifecycleState == AndroidViewLifecycleState.DismissingAllowingStateLoss) && state is ICancelableRequest request)
                    _mediator.OnClosing(request, metadata);
                else if (lifecycleState == AndroidViewLifecycleState.Dismissed || lifecycleState == AndroidViewLifecycleState.DismissedAllowingStateLoss || lifecycleState == AndroidViewLifecycleState.Canceled)
                    _mediator.OnClosed(metadata);
            }

            #endregion
        }

        #endregion
    }
}