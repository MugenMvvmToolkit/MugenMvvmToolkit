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
using MugenMvvm.Presenters;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestViewModelPresenterMediatorBase<T> : ViewModelPresenterMediatorBase<T> where T : class
    {
        #region Fields

        public ThreadExecutionMode? ExecutionModeField;
        public NavigationType NavigationTypeField = NavigationType.Popup;

        #endregion

        #region Constructors

        public TestViewModelPresenterMediatorBase(IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null)
            : base(viewManager, wrapperManager, navigationDispatcher, threadDispatcher)
        {
        }

        #endregion

        #region Properties

        public new IViewModelBase ViewModel => base.ViewModel;

        public new IViewModelViewMapping Mapping => base.Mapping;

        public new IView? View => base.View;

        public new T? CurrentView => base.CurrentView;

        public override NavigationType NavigationType => NavigationTypeField;

        protected override ThreadExecutionMode ExecutionMode => ExecutionModeField ?? base.ExecutionMode;

        public Func<INavigationContext, bool>? ActivateViewHandler { get; set; }

        public Func<NavigationMode, IReadOnlyMetadataContext?, INavigationContext>? GetNavigationContextHandler { get; set; }

        public Func<bool, IReadOnlyMetadataContext?, IPresenterResult>? GetPresenterResultHandler { get; set; }

        public Func<object?, IReadOnlyMetadataContext?, NavigationMode>? GetShowNavigationModeHandler { get; set; }

        public Action<IReadOnlyMetadataContext?>? OnInitializedHandler { get; set; }

        public Func<INavigationContext, bool>? OnNavigatedHandler { get; set; }

        public Func<INavigationContext, CancellationToken, bool>? OnNavigationCanceledHandler { get; set; }

        public Func<INavigationContext, Exception, bool>? OnNavigationFailedHandler { get; set; }

        public Func<CancellationToken, IReadOnlyMetadataContext?, Task?>? WaitNavigationBeforeCloseAsyncHandler { get; set; }

        public Func<CancellationToken, IReadOnlyMetadataContext?, Task?>? WaitNavigationBeforeShowAsyncHandler { get; set; }

        public Action<INavigationContext>? ShowViewHandler { get; set; }

        public Action<INavigationContext>? InitializeViewHandler { get; set; }

        public Action<INavigationContext>? CloseViewHandler { get; set; }

        public Action<INavigationContext>? CleanupViewHandler { get; set; }

        #endregion

        #region Methods

        protected override bool ActivateView(T view, INavigationContext context)
        {
            return ActivateViewHandler?.Invoke(context) ?? base.ActivateView(view, context);
        }

        protected override INavigationContext GetNavigationContext(NavigationMode mode, IReadOnlyMetadataContext? metadata)
        {
            return GetNavigationContextHandler?.Invoke(mode, metadata) ?? base.GetNavigationContext(mode, metadata);
        }

        protected override IPresenterResult GetPresenterResult(bool show, IReadOnlyMetadataContext? metadata)
        {
            return GetPresenterResultHandler?.Invoke(show, metadata) ?? base.GetPresenterResult(show, metadata);
        }

        protected override NavigationMode GetShowNavigationMode(object? view, IReadOnlyMetadataContext? metadata)
        {
            return GetShowNavigationModeHandler?.Invoke(view, metadata) ?? base.GetShowNavigationMode(view, metadata);
        }

        protected override void OnInitialized(IReadOnlyMetadataContext? metadata)
        {
            OnInitializedHandler?.Invoke(metadata);
            base.OnInitialized(metadata);
        }

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

        protected override Task WaitNavigationBeforeCloseAsync(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return WaitNavigationBeforeCloseAsyncHandler?.Invoke(cancellationToken, metadata) ?? base.WaitNavigationBeforeCloseAsync(cancellationToken, metadata);
        }

        protected override Task WaitNavigationBeforeShowAsync(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return WaitNavigationBeforeShowAsyncHandler?.Invoke(cancellationToken, metadata) ?? base.WaitNavigationBeforeShowAsync(view, cancellationToken, metadata);
        }

        protected override void ShowView(T view, INavigationContext context)
        {
            ShowViewHandler?.Invoke(context);
        }

        protected override void InitializeView(T view, INavigationContext context)
        {
            InitializeViewHandler?.Invoke(context);
        }

        protected override void CloseView(T view, INavigationContext context)
        {
            CloseViewHandler?.Invoke(context);
        }

        protected override void CleanupView(T view, INavigationContext context)
        {
            CleanupViewHandler?.Invoke(context);
        }

        #endregion
    }
}