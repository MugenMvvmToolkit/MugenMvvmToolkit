#region Copyright

// ****************************************************************************
// <copyright file="NavigationProviderTest.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.Test.TestViewModels;
using MugenMvvmToolkit.UWP.Infrastructure.Navigation;
using MugenMvvmToolkit.WPF.Infrastructure.Navigation;
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
                s.ShouldContain(typeof(NavigableViewModelMock).AssemblyQualifiedName);

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
            NavigationProvider.NavigateAsync(dataContext);
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
                s.ShouldContain(typeof(NavigableViewModelMock).AssemblyQualifiedName);

                d.ShouldNotBeNull();
                item.ShouldEqual(PageMapping);
                isInvokedNavigate = true;
                return true;
            };

            NavigationProvider.NavigateAsync(new DataContext(NavigationConstants.ViewModel.ToValue(viewModel)));
            isInvoked.ShouldBeTrue();
            isInvokedNavigate.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldUpdateCanCloseMetadata()
        {
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            var testView = new ViewMock();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            testView.DataContext = viewModel;

            viewModel.Settings.Metadata.GetData(ViewModelConstants.CanCloseHandler).ShouldBeNull();
            NavigationService.OnNavigated(new NavigationEventArgsMock(testView, NavigationMode.New));

            viewModel.Settings.Metadata.GetData(ViewModelConstants.CanCloseHandler).Invoke(viewModel, null).ShouldBeFalse();
            NavigationService.CanClose = (model, context) => true;
            viewModel.Settings.Metadata.GetData(ViewModelConstants.CanCloseHandler).Invoke(viewModel, null).ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldNotCloseViewModelIfItIsNotOpened()
        {
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            var viewModel = GetViewModel<NavigableViewModelMock>();

            bool isInvoked = false;
            NavigationService.CanClose = (model, context) => true;
            NavigationService.TryClose = (m, c) =>
            {
                m.ShouldEqual(viewModel);
                isInvoked = true;
                return true;
            };
            NavigationProvider.TryCloseAsync(new DataContext(NavigationConstants.ViewModel.ToValue(viewModel)));
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ProviderShouldCloseViewModel()
        {
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            var testView = new ViewMock();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            testView.DataContext = viewModel;
            NavigationService.OnNavigated(new NavigationEventArgsMock(testView, NavigationMode.New));

            bool isInvoked = false;
            NavigationService.CanClose = (model, context) => true;
            NavigationService.TryClose = (m, c) =>
            {
                m.ShouldEqual(viewModel);
                isInvoked = true;
                return true;
            };
            NavigationProvider.TryCloseAsync(new DataContext(NavigationConstants.ViewModel.ToValue(viewModel)));
            isInvoked.ShouldBeTrue();
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

            NavigationDispatcher.OnNavigatingFromAsync = context =>
            {
                isCancelInvoked = true;
                context.NavigationProvider.ShouldEqual(NavigationProvider);
                context.NavigationMode.ShouldEqual(mode);
                return ToolkitExtensions.FromResult(false);
            };
            NavigationDispatcher.OnNavigated = context => isNavigatedInvoked = true;

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

            NavigationDispatcher.OnNavigatingFromAsync = context =>
            {
                isCancelInvoked = true;
                context.NavigationProvider.ShouldEqual(NavigationProvider);
                context.NavigationMode.ShouldEqual(mode);
                return ToolkitExtensions.FromResult(false);
            };
            NavigationDispatcher.OnNavigated = context => isNavigatedInvoked = true;

            var cancelArgs = new NavigatingCancelEventArgsMock(mode, false) { Cancel = false };
            NavigationService.OnNavigating(cancelArgs);

            isCancelInvoked.ShouldBeFalse();
            isNavigatedInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void ProviderShouldCallOnNavigatedMethod()
        {
            bool isNavigatedInvoked = false;
            const NavigationMode mode = NavigationMode.Back;
            ViewPageMappingProvider.FindMappingForView = (type, b) => PageMapping;
            NavigationProvider.CurrentViewModel.ShouldBeNull();
            var viewModel = GetViewModel<NavigableViewModelMock>();
            NavigationDispatcher.OnNavigated = context =>
                    {
                        isNavigatedInvoked = true;
                        context.NavigationProvider.ShouldEqual(NavigationProvider);
                        context.NavigationMode.ShouldEqual(mode);
                    };

            NavigationService.OnNavigated(new NavigationEventArgsMock(new ViewMock { DataContext = viewModel }, mode));
            isNavigatedInvoked.ShouldBeTrue();
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
            NavigationProvider = new NavigationProvider(NavigationService, ThreadManager, ViewPageMappingProvider, ViewManager, ViewModelProvider, NavigationDispatcher, new EventAggregator(), null);
        }

        #endregion
    }
}
