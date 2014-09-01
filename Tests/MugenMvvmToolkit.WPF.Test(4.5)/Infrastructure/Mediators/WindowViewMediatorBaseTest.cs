using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.Test.TestViewModels;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Mediators
{
    [TestClass]
    public abstract class WindowViewMediatorBaseTest<TView> : TestBase
        where TView : class, IWindowViewBase
    {
        #region Methods

        [TestMethod]
        public virtual void ViewModelShouldBeInitialized()
        {
            var vm = GetViewModel<NavigableViewModelMock>();
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager, OperationCallbackManager);
            windowMediator.ViewModel.ShouldEqual(vm);
        }

        [TestMethod]
        public virtual void MediatorShouldPassDataContextToViewManager()
        {
            bool isInvoked = false;
            var dataContext = new DataContext();
            var vm = GetViewModel<NavigableViewModelMock>();
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager, OperationCallbackManager);
            ViewManager.GetViewRawDelegate = (model, context) =>
            {
                isInvoked = true;
                context.ShouldEqual(dataContext);
                model.ShouldEqual(vm);
                return new DialogViewMock();
            };

            windowMediator.Show(null, dataContext);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void IsOpenShouldBeTrueWhenWindowIsShowed()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager,
                OperationCallbackManager);
            windowMediator.IsOpen.ShouldBeFalse();
            view.IsShowAny.ShouldBeFalse();
            windowMediator.Show(null, DataContext.Empty);
            windowMediator.IsOpen.ShouldBeTrue();
            view.IsShowAny.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void ShowTwiceShouldThrowException()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager, OperationCallbackManager);
            windowMediator.Show(null, DataContext.Empty);
            ShouldThrow(() => windowMediator.Show(null, DataContext.Empty));
        }

        [TestMethod]
        public virtual void CloseEmptyWindowShouldThrowException()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager, OperationCallbackManager);
            ShouldThrow(() => windowMediator.CloseAsync(null));
        }

        [TestMethod]
        public virtual void ViewShouldBeInitializedWhenWindowIsShowed()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager,
                OperationCallbackManager);
            windowMediator.View.ShouldBeNull();
            windowMediator.Show(null, DataContext.Empty);
            windowMediator.View.ShouldEqual((TView)(object)view);
        }

        [TestMethod]
        public virtual void ViewShouldBeNullAfterWindowClosed()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager, OperationCallbackManager);
            windowMediator.View.ShouldBeNull();
            windowMediator.Show(null, DataContext.Empty);
            windowMediator.View.ShouldEqual((TView)(object)view);
            windowMediator.CloseAsync(null);
            windowMediator.View.ShouldBeNull();
        }

        [TestMethod]
        public virtual void ShowCallbackShouldBeInvokedAfterWindowClosed()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager, OperationCallbackManager);

            var mockCallback = new OperationCallbackMock();
            bool isRegistered = false;
            IOperationResult operationResult = null;
            OperationCallbackManager.Register = (type, o, arg3, arg4) =>
            {
                type.ShouldEqual(OperationType.WindowNavigation);
                o.ShouldEqual(vm);
                arg3.ShouldEqual(mockCallback);
                isRegistered = true;
            };
            OperationCallbackManager.SetResult = (o, result) =>
            {
                o.ShouldEqual(vm);
                (result.OperationContext is INavigationContext).ShouldBeTrue();
                operationResult = result;
            };

            windowMediator.Show(mockCallback, DataContext.Empty);
            isRegistered.ShouldBeTrue();
            windowMediator.CloseAsync(null).Result.ShouldBeTrue();
            operationResult.ShouldNotBeNull();
            operationResult.Result.ShouldBeNull();
        }

        [TestMethod]
        public virtual void ShowCallbackShouldBeInvokedAfterWindowClosedTrueResult()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager, OperationCallbackManager);

            var mockCallback = new OperationCallbackMock();
            bool isRegistered = false;
            IOperationResult operationResult = null;
            OperationCallbackManager.Register = (type, o, arg3, arg4) =>
            {
                type.ShouldEqual(OperationType.WindowNavigation);
                o.ShouldEqual(vm);
                arg3.ShouldEqual(mockCallback);
                isRegistered = true;
            };
            OperationCallbackManager.SetResult = (o, result) =>
            {
                o.ShouldEqual(vm);
                (result.OperationContext is INavigationContext).ShouldBeTrue();
                operationResult = result;
            };

            windowMediator.Show(mockCallback, DataContext.Empty);
            isRegistered.ShouldBeTrue();

            vm.OperationResult = true;
            windowMediator.CloseAsync(null).Result.ShouldBeTrue();
            operationResult.ShouldNotBeNull();
            operationResult.Result.ShouldEqual(true);
        }

        [TestMethod]
        public virtual void MediatorShouldCallCloseAsynMethodOnClose()
        {
            bool result = false;
            bool isInvoked = false;
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager, OperationCallbackManager);
            vm.CloseDelegate = o =>
            {
                isInvoked = true;
                return MvvmExtensions.FromResult(result);
            };
            windowMediator.Show(null, DataContext.Empty);
            windowMediator.CloseAsync(null).Result.ShouldBeFalse();
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            result = true;
            windowMediator.CloseAsync(null).Result.ShouldBeTrue();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void ClosedEventFromViewModelShouldCloseWindow()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager,
                OperationCallbackManager);

            IOperationResult operationResult = null;
            OperationCallbackManager.SetResult = (o, result) => operationResult = result;

            windowMediator.Show(null, DataContext.Empty);
            vm.OnClosed(new ViewModelClosedEventArgs(vm, null));
            operationResult.ShouldNotBeNull();
            windowMediator.IsOpen.ShouldBeFalse();
        }

        [TestMethod]
        public virtual void ClosingEventFromViewShouldCloseWindow()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager,
                OperationCallbackManager);

            IOperationResult operationResult = null;
            OperationCallbackManager.SetResult = (o, result) => operationResult = result;

            windowMediator.Show(null, DataContext.Empty);
            view.Close();
            operationResult.ShouldNotBeNull();
            windowMediator.IsOpen.ShouldBeFalse();
        }

        [TestMethod]
        public virtual void MediatorShouldCallOnNavigatedToMethodOnShow()
        {
            bool isInvoked = false;
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager,
                OperationCallbackManager);
            vm.OnNavigatedToDelegate = context =>
            {
                isInvoked = true;
                context.ShouldNotBeNull();
            };
            windowMediator.Show(null, DataContext.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void MediatorShouldCallOnNavigatingFromMethodOnClose()
        {
            bool result = false;
            bool isInvoked = false;
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager,
                OperationCallbackManager);
            vm.OnNavigatingFromDelegate = o =>
            {
                o.ShouldNotBeNull();
                isInvoked = true;
                return MvvmExtensions.FromResult(result);
            };
            windowMediator.Show(null, DataContext.Empty);
            windowMediator.CloseAsync(null).Result.ShouldBeFalse();
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            result = true;
            windowMediator.CloseAsync(null).Result.ShouldBeTrue();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void MediatorShouldCallOnNavigatedFromMethodOnClose()
        {
            bool isInvoked = false;
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm, ThreadManager, ViewManager, OperationCallbackManager);
            vm.OnNavigatedFromDelegate = o =>
            {
                o.ShouldNotBeNull();
                isInvoked = true;
            };
            windowMediator.Show(null, DataContext.Empty);
            windowMediator.CloseAsync(null).Result.ShouldBeTrue();
            isInvoked.ShouldBeTrue();
        }

        protected abstract WindowViewMediatorBase<TView> Create(IViewModel viewModel, IThreadManager threadManager,
            IViewManager viewManager, IOperationCallbackManager callbackManager);

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            base.OnInit();
            ThreadManager.ImmediateInvokeOnUiThread = true;
            ThreadManager.ImmediateInvokeAsync = true;
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
        }

        #endregion
    }
}