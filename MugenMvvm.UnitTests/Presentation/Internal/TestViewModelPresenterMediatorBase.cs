using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;
using MugenMvvm.Presentation;

namespace MugenMvvm.UnitTests.Presentation.Internal
{
    public class TestViewModelPresenterMediatorBase<T> : ViewModelPresenterMediatorBase<T> where T : class
    {
        public ThreadExecutionMode? ExecutionModeField;
        public NavigationType NavigationTypeField = NavigationType.Popup;

        public TestViewModelPresenterMediatorBase(IViewModelBase viewModel, IViewMapping mapping, IViewManager? viewManager = null, IWrapperManager? wrapperManager = null,
            INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null, IViewModelManager? viewModelManager = null)
            : base(viewModel, mapping, viewManager, wrapperManager, navigationDispatcher, threadDispatcher, viewModelManager)
        {
        }

        public override NavigationType NavigationType => NavigationTypeField;

        public new T? CurrentView => base.CurrentView;

        public Func<INavigationContext, ValueTask<bool>>? ActivateViewHandler { get; set; }

        public Func<NavigationMode, IReadOnlyMetadataContext?, INavigationContext>? GetNavigationContextHandler { get; set; }

        public Func<bool, IReadOnlyMetadataContext?, IPresenterResult>? GetPresenterResultHandler { get; set; }

        public Func<INavigationContext, bool>? OnNavigatedHandler { get; set; }

        public Func<INavigationContext, CancellationToken, bool>? OnNavigationCanceledHandler { get; set; }

        public Func<INavigationContext, Exception, bool>? OnNavigationFailedHandler { get; set; }

        public Func<INavigationContext, Task?>? ShowViewHandler { get; set; }

        public Action<INavigationContext>? InitializeViewHandler { get; set; }

        public Func<INavigationContext, Task?>? CloseViewHandler { get; set; }

        public Action<INavigationContext>? CleanupViewHandler { get; set; }

        protected override ThreadExecutionMode ExecutionMode => ExecutionModeField ?? base.ExecutionMode;

        protected override ValueTask<bool> ActivateViewAsync(T view, INavigationContext context, CancellationToken cancellationToken) =>
            ActivateViewHandler?.Invoke(context) ?? base.ActivateViewAsync(view, context, cancellationToken);

        protected override INavigationContext GetNavigationContext(NavigationMode mode, IReadOnlyMetadataContext? metadata) =>
            GetNavigationContextHandler?.Invoke(mode, metadata) ?? base.GetNavigationContext(mode, metadata);

        protected override IPresenterResult GetPresenterResult(bool show, IReadOnlyMetadataContext? metadata) =>
            GetPresenterResultHandler?.Invoke(show, metadata) ?? base.GetPresenterResult(show, metadata);

        protected override void OnNavigated(INavigationContext navigationContext)
        {
            if (OnNavigatedHandler == null || OnNavigatedHandler.Invoke(navigationContext))
                base.OnNavigated(navigationContext);
        }

        protected override void OnNavigationCanceled(INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (OnNavigationCanceledHandler == null || OnNavigationCanceledHandler.Invoke(navigationContext, cancellationToken))
                base.OnNavigationCanceled(navigationContext, cancellationToken);
        }

        protected override void OnNavigationFailed(INavigationContext navigationContext, Exception exception)
        {
            if (OnNavigationFailedHandler == null || OnNavigationFailedHandler.Invoke(navigationContext, exception))
                base.OnNavigationFailed(navigationContext, exception);
        }

        protected override Task ShowViewAsync(T view, INavigationContext context, CancellationToken cancellationToken) => ShowViewHandler?.Invoke(context) ?? Task.CompletedTask;

        protected override void InitializeView(T view, INavigationContext context) => InitializeViewHandler?.Invoke(context);

        protected override Task CloseViewAsync(T view, INavigationContext context, CancellationToken cancellationToken) => CloseViewHandler?.Invoke(context) ?? Task.CompletedTask;

        protected override void CleanupView(T view, INavigationContext context) => CleanupViewHandler?.Invoke(context);
    }
}