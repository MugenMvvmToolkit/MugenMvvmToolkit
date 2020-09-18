using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;
using MugenMvvm.Presenters;

namespace MugenMvvm.UnitTests.Presenters.Internal
{
    public class TestViewModelPresenterMediatorBase<T> : ViewModelPresenterMediatorBase<T> where T : class
    {
        #region Fields

        public ThreadExecutionMode? ExecutionModeField;
        public NavigationType NavigationTypeField = NavigationType.Popup;

        #endregion

        #region Constructors

        public TestViewModelPresenterMediatorBase(IViewModelBase viewModel, IViewMapping mapping, IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null)
            : base(viewModel, mapping, viewManager, wrapperManager, navigationDispatcher, threadDispatcher)
        {
        }

        #endregion

        #region Properties

        public new T? CurrentView => base.CurrentView;

        public override NavigationType NavigationType => NavigationTypeField;

        protected override ThreadExecutionMode ExecutionMode => ExecutionModeField ?? base.ExecutionMode;

        public Func<INavigationContext, Task<bool>>? ActivateViewHandler { get; set; }

        public Func<NavigationMode, IReadOnlyMetadataContext?, INavigationContext>? GetNavigationContextHandler { get; set; }

        public Func<bool, IReadOnlyMetadataContext?, IPresenterResult>? GetPresenterResultHandler { get; set; }

        public Func<object?, IReadOnlyMetadataContext?, NavigationMode>? GetShowNavigationModeHandler { get; set; }


        public Func<INavigationContext, bool>? OnNavigatedHandler { get; set; }

        public Func<INavigationContext, CancellationToken, bool>? OnNavigationCanceledHandler { get; set; }

        public Func<INavigationContext, Exception, bool>? OnNavigationFailedHandler { get; set; }

        public Func<INavigationContext, Task?>? ShowViewHandler { get; set; }

        public Action<INavigationContext>? InitializeViewHandler { get; set; }

        public Func<INavigationContext, Task?>? CloseViewHandler { get; set; }

        public Action<INavigationContext>? CleanupViewHandler { get; set; }

        #endregion

        #region Methods

        protected override Task<bool> ActivateViewAsync(T view, INavigationContext context) => ActivateViewHandler?.Invoke(context) ?? base.ActivateViewAsync(view, context);

        protected override INavigationContext GetNavigationContext(NavigationMode mode, IReadOnlyMetadataContext? metadata) =>
            GetNavigationContextHandler?.Invoke(mode, metadata) ?? base.GetNavigationContext(mode, metadata);

        protected override IPresenterResult GetPresenterResult(bool show, IReadOnlyMetadataContext? metadata) => GetPresenterResultHandler?.Invoke(show, metadata) ?? base.GetPresenterResult(show, metadata);

        protected override NavigationMode GetShowNavigationMode(object? view, IReadOnlyMetadataContext? metadata) => GetShowNavigationModeHandler?.Invoke(view, metadata) ?? base.GetShowNavigationMode(view, metadata);

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

        protected override Task ShowViewAsync(T view, INavigationContext context) => ShowViewHandler?.Invoke(context) ?? Default.CompletedTask;

        protected override void InitializeView(T view, INavigationContext context) => InitializeViewHandler?.Invoke(context);

        protected override Task CloseViewAsync(T view, INavigationContext context) => CloseViewHandler?.Invoke(context) ?? Default.CompletedTask;

        protected override void CleanupView(T view, INavigationContext context) => CleanupViewHandler?.Invoke(context);

        #endregion
    }
}