using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.Test.TestViewModels;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Navigation
{
    [TestClass]
    public class NavigationProviderTest : TestBase
    {
        #region Fields

        private static readonly Uri Uri = new Uri("/app;component/Test/view.xaml", UriKind.Relative);

        private static readonly ViewMappingItem PageMapping = new ViewMappingItem(
            typeof(NavigableViewModelMock), typeof(ViewMock), null, Uri);

        #endregion

        #region Properties

        protected ViewPageMappingProviderMock ViewPageMappingProvider { get; set; }

        protected NavigationServiceMock NavigationService { get; set; }

        protected NavigationProvider NavigationProvider { get; set; }

        #endregion

        #region Test methods

        [TestMethod]
        public void ProviderShouldReturnsCurrentViewModelFromNavigationService()
        {
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            NavigationProvider.CurrentViewModel.ShouldBeNull();
            var viewModel = GetViewModel<NavigableViewModelMock>();

            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, NavigationMode.New));
            NavigationProvider.CurrentViewModel.ShouldEqual(viewModel);
        }
        
        [TestMethod]
        public void ProviderShouldNavigateToViewModelWithViewName()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            bool isInvoked = false;
            bool isInvokedNavigate = false;
            const string viewName = "test";
            var viewModel = GetViewModel<NavigableViewModelMock>();
            ViewPageMappingProvider.FindMappingForViewModel = (type, s, arg3) =>
            {
                viewModel.ShouldBeType(type);
                s.ShouldEqual(viewName);
                arg3.ShouldBeTrue();
                isInvoked = true;
                return PageMapping;
            };
            NavigationService.Navigate = (item, o, d) =>
            {
                var s = o as string;
                s.ShouldNotBeNull();
                s.ShouldEqual(typeof(NavigableViewModelMock).AssemblyQualifiedName);

                d.ShouldNotBeNull();
                item.ShouldEqual(PageMapping);
                isInvokedNavigate = true;
                return true;
            };

            var dataContext = new DataContext
            {
                {NavigationConstants.ViewModel, viewModel},
                {NavigationConstants.ViewName, viewName}
            };
            NavigationProvider.Navigate(new OperationCallbackMock(), dataContext);
            isInvoked.ShouldBeTrue();
            isInvokedNavigate.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldNavigateToViewModelWithoutParameters()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            bool isInvoked = false;
            bool isInvokedNavigate = false;
            var viewModel = GetViewModel<NavigableViewModelMock>();
            ViewPageMappingProvider.FindMappingForViewModel = (type, s, arg3) =>
            {
                viewModel.ShouldBeType(type);
                s.ShouldBeNull();
                arg3.ShouldBeTrue();
                isInvoked = true;
                return PageMapping;
            };
            NavigationService.Navigate = (item, o, d) =>
            {
                var s = o as string;
                s.ShouldNotBeNull();
                s.ShouldEqual(typeof(NavigableViewModelMock).AssemblyQualifiedName);

                d.ShouldNotBeNull();
                item.ShouldEqual(PageMapping);
                isInvokedNavigate = true;
                return true;
            };

            NavigationProvider.Navigate(new OperationCallbackMock(), new DataContext(NavigationConstants.ViewModel.ToValue(viewModel)));
            isInvoked.ShouldBeTrue();
            isInvokedNavigate.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldNavigateToViewModelWithParameters()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            bool isInvoked = false;
            bool isInvokedNavigate = false;
            var viewModel = GetViewModel<NavigableViewModelMock>();
            ViewPageMappingProvider.FindMappingForViewModel = (type, s, arg3) =>
            {
                viewModel.ShouldBeType(type);
                s.ShouldBeNull();
                arg3.ShouldBeTrue();
                isInvoked = true;
                return PageMapping;
            };
            NavigationService.Navigate = (item, o, d) =>
            {
                (o as IDataContext).ShouldNotBeNull();
                d.ShouldNotBeNull();
                item.ShouldEqual(PageMapping);
                isInvokedNavigate = true;
                return true;
            };

            var dataContext = new DataContext
            {
                {NavigationConstants.ViewModel, viewModel},
                {NavigationConstants.Parameters, new DataContext()}
            };
            NavigationProvider.Navigate(new OperationCallbackMock(), dataContext);
            isInvoked.ShouldBeTrue();
            isInvokedNavigate.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldNavigateToViewModelAndRegisterCallback()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            bool isInvoked = false;
            var callbackMock = new OperationCallbackMock();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            object param = null;
            ViewPageMappingProvider.FindMappingForViewModel = (type, s, arg3) => PageMapping;
            NavigationService.Navigate = (item, o, d) =>
            {
                param = o;
                return true;
            };
            NavigationService.GetParameterFromArgs = args => param;
            OperationCallbackManager.Register = (type, o, arg3, arg4) =>
            {
                type.ShouldEqual(OperationType.PageNavigation);
                o.ShouldEqual(viewModel);
                arg3.ShouldEqual(callbackMock);
                isInvoked = true;
            };
            NavigationProvider.Navigate(callbackMock, new DataContext(NavigationConstants.ViewModel.ToValue(viewModel)));
            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock(), NavigationMode.New));
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldUpdateCloseCommandToGoBack()
        {
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            var testView = new ViewMock();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            testView.DataContext = viewModel;

            viewModel.CloseCommand.ShouldBeNull();
            NavigationService.OnNavigated(new NavigationEventArgsMock(testView, NavigationMode.New));

            viewModel.CloseCommand.CanExecute(null).ShouldBeFalse();
            NavigationService.CanGoBack = true;
            viewModel.CloseCommand.CanExecute(null).ShouldBeTrue();

            bool isInvoked = false;
            NavigationService.GoBack = () => isInvoked = true;
            viewModel.CloseCommand.Execute(null);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldUpdateCloseCommandToOldValue()
        {
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            var relayCommandMock = new RelayCommandMock();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            viewModel.CloseCommand = relayCommandMock;
            

            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, NavigationMode.New));
            viewModel.CloseCommand.ShouldNotEqual(relayCommandMock);

            NavigationService.OnNavigated(new NavigationEventArgsMock(null, NavigationMode.New));
            viewModel.CloseCommand.ShouldEqual(relayCommandMock);
        }

        [TestMethod]
        public void ProviderShouldCallOnNavigatingFromMethod()
        {
            bool isCancelInvoked = false;
            bool isNavigatedInvoked = false;
            const NavigationMode mode = NavigationMode.Refresh;
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            NavigationProvider.CurrentViewModel.ShouldBeNull();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, NavigationMode.New));

            viewModel.OnNavigatingFromDelegate = context =>
            {
                isCancelInvoked = true;
                context.NavigationProvider.ShouldEqual(NavigationProvider);
                context.NavigationMode.ShouldEqual(mode);
                return ToolkitExtensions.FromResult(false);
            };
            viewModel.OnNavigatedFromDelegate = context => isNavigatedInvoked = true;

            var cancelArgs = new NavigatingCancelEventArgsMock(mode, true) { Cancel = false };
            NavigationService.OnNavigating(cancelArgs);
            cancelArgs.Cancel.ShouldBeTrue();
            isCancelInvoked.ShouldBeTrue();
            isNavigatedInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ProviderShouldNotCallOnNavigatingFromMethodNotCancelable()
        {
            bool isCancelInvoked = false;
            bool isNavigatedInvoked = false;
            const NavigationMode mode = NavigationMode.Refresh;
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            NavigationProvider.CurrentViewModel.ShouldBeNull();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, NavigationMode.New));

            viewModel.OnNavigatingFromDelegate = context =>
            {
                isCancelInvoked = true;
                context.NavigationProvider.ShouldEqual(NavigationProvider);
                context.NavigationMode.ShouldEqual(mode);
                return ToolkitExtensions.FromResult(false);
            };
            viewModel.OnNavigatedFromDelegate = context => isNavigatedInvoked = true;

            var cancelArgs = new NavigatingCancelEventArgsMock(mode, false) { Cancel = false };
            NavigationService.OnNavigating(cancelArgs);

            isCancelInvoked.ShouldBeFalse();
            isNavigatedInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ProviderShouldNotCallCloseAsyncMethodModeNotEqBack()
        {
            bool isCancelInvoked = false;
            bool isNavigatedInvoked = false;
            const NavigationMode mode = NavigationMode.Refresh;
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            NavigationProvider.CurrentViewModel.ShouldBeNull();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, NavigationMode.New));

            viewModel.CloseDelegate = obj =>
            {
                isCancelInvoked = true;
                var context = (INavigationContext)obj;
                context.NavigationProvider.ShouldEqual(NavigationProvider);
                context.NavigationMode.ShouldEqual(mode);
                return ToolkitExtensions.FromResult(false);
            };
            viewModel.OnNavigatedFromDelegate = context => isNavigatedInvoked = true;

            var cancelArgs = new NavigatingCancelEventArgsMock(mode, true) { Cancel = false };
            NavigationService.OnNavigating(cancelArgs);
            isCancelInvoked.ShouldBeFalse();
            isNavigatedInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ProviderShouldCallCloseAsyncMethodOnBackNavigation()
        {
            bool isCancelInvoked = false;
            bool isNavigatedInvoked = false;
            const NavigationMode mode = NavigationMode.Back;
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            NavigationProvider.CurrentViewModel.ShouldBeNull();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, NavigationMode.New));

            viewModel.CloseDelegate = obj =>
            {
                isCancelInvoked = true;
                var context = (INavigationContext)obj;
                context.NavigationProvider.ShouldEqual(NavigationProvider);
                context.NavigationMode.ShouldEqual(mode);
                return ToolkitExtensions.FromResult(false);
            };
            viewModel.OnNavigatedFromDelegate = context => isNavigatedInvoked = true;

            var cancelArgs = new NavigatingCancelEventArgsMock(mode, true) { Cancel = false };
            NavigationService.OnNavigating(cancelArgs);
            cancelArgs.Cancel.ShouldBeTrue();
            isCancelInvoked.ShouldBeTrue();
            isNavigatedInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ProviderShouldGoBackIfViewModelClosedEvent()
        {
            bool isGoBackInvoked = false;
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            NavigationProvider.CurrentViewModel.ShouldBeNull();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, NavigationMode.New));

            NavigationService.CanGoBack = true;
            NavigationService.GoBack = () => isGoBackInvoked = true;

            isGoBackInvoked.ShouldBeFalse();
            viewModel.OnClosed(new ViewModelClosedEventArgs(viewModel, viewModel));
            isGoBackInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldCallOnNavigatedToMethod()
        {
            bool isNavigatedInvoked = false;
            const NavigationMode mode = NavigationMode.Back;
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            NavigationProvider.CurrentViewModel.ShouldBeNull();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            viewModel.OnNavigatedToDelegate = context =>
            {
                isNavigatedInvoked = true;
                context.NavigationProvider.ShouldEqual(NavigationProvider);
                context.NavigationMode.ShouldEqual(mode);
            };

            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, mode));
            isNavigatedInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldCallOnNavigatedFromMethod()
        {
            bool isNavigatedInvoked = false;
            const NavigationMode mode = NavigationMode.Back;
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            NavigationProvider.CurrentViewModel.ShouldBeNull();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            viewModel.OnNavigatedFromDelegate = context =>
            {
                isNavigatedInvoked = true;
                context.NavigationProvider.ShouldEqual(NavigationProvider);
                context.NavigationMode.ShouldEqual(mode);
            };

            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, mode));
            isNavigatedInvoked.ShouldBeFalse();

            NavigationService.OnNavigated(new NavigationEventArgsMock(null, mode));
        }

        [TestMethod]
        public void ProviderShouldCompleteCallbackOnBackNavigation()
        {
            bool isInvoked = false;
            var callbackMock = new OperationCallbackMock();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            object param = null;
            ViewPageMappingProvider.FindMappingForViewModel = (type, s, arg3) => PageMapping;
            NavigationService.Navigate = (item, o, d) =>
            {
                param = o;
                return true;
            };
            NavigationService.GetParameterFromArgs = args => param;
            OperationCallbackManager.Register = (type, o, arg3, arg4) => { };
            OperationCallbackManager.SetResult = (o, result) =>
            {
                isInvoked = true;
            };
            NavigationProvider.Navigate(callbackMock, new DataContext(NavigationConstants.ViewModel.ToValue(viewModel)));
            NavigationService.OnNavigated(new NavigationEventArgsMock(null, NavigationMode.New));

            NavigationService.OnNavigated(new NavigationEventArgsMock(null, NavigationMode.Back));
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldNotCompleteCallbackNotOnBackNavigation()
        {
            bool isInvoked = false;
            var callbackMock = new OperationCallbackMock();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            object param = null;
            ViewPageMappingProvider.FindMappingForViewModel = (type, s, arg3) => PageMapping;
            NavigationService.Navigate = (item, o, d) =>
            {
                param = o;
                return true;
            };
            NavigationService.GetParameterFromArgs = args => param;
            OperationCallbackManager.Register = (type, o, arg3, arg4) => { };
            OperationCallbackManager.SetResult = (o, result) =>
            {
                isInvoked = true;
            };
            NavigationProvider.Navigate(callbackMock, new DataContext(NavigationConstants.ViewModel.ToValue(viewModel)));
            NavigationService.OnNavigated(new NavigationEventArgsMock(null, NavigationMode.New));

            NavigationService.OnNavigated(new NavigationEventArgsMock(null, NavigationMode.Refresh));
            isInvoked.ShouldBeFalse();
        }

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            ThreadManager.ImmediateInvokeAsync = true;
            ThreadManager.ImmediateInvokeOnUiThread = true;
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            base.OnInit();
            NavigationService = new NavigationServiceMock();
            ViewPageMappingProvider = new ViewPageMappingProviderMock();
            NavigationProvider = new NavigationProvider(NavigationService, ThreadManager, ViewPageMappingProvider,
                ViewManager, ViewModelProvider, OperationCallbackManager);
        }

        #endregion
    }
}