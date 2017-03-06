#region Copyright

// ****************************************************************************
// <copyright file="WindowViewMediatorBaseTest.cs">
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

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
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
        where TView : class
    {
        #region Fields

        private WrapperManager _wrapperManager;

        #endregion

        #region Methods

        [TestMethod]
        public virtual void ViewModelShouldBeInitialized()
        {
            var vm = GetViewModel<NavigableViewModelMock>();
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            windowMediator.ViewModel.ShouldEqual(vm);
        }

        [TestMethod]
        public virtual void MediatorShouldPassDataContextToViewManager()
        {
            bool isInvoked = false;
            var dataContext = new DataContext();
            var vm = GetViewModel<NavigableViewModelMock>();
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            ViewManager.GetViewRawDelegate = (model, context) =>
            {
                isInvoked = true;
                context.ShouldEqual(dataContext);
                model.ShouldEqual(vm);
                return new DialogViewMock();
            };

            windowMediator.ShowAsync(dataContext);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void IsOpenShouldBeTrueWhenWindowIsShowed()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            windowMediator.IsOpen.ShouldBeFalse();
            view.IsShowAny.ShouldBeFalse();
            windowMediator.ShowAsync(DataContext.Empty);
            windowMediator.IsOpen.ShouldBeTrue();
            view.IsShowAny.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void ShowTwiceShouldActivateWindow()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            windowMediator.ShowAsync(DataContext.Empty);
            view.IsActivated.ShouldBeFalse();
            windowMediator.ShowAsync(DataContext.Empty);
            view.IsActivated.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void CloseEmptyWindowShouldNotThrowException()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            windowMediator.CloseAsync(null);
        }

        [TestMethod]
        public virtual void ViewShouldBeInitializedWhenWindowIsShowed()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            windowMediator.View.ShouldBeNull();
            windowMediator.ShowAsync(DataContext.Empty);
            windowMediator.View.ShouldEqual((TView)(object)view);
        }

        [TestMethod]
        public virtual void ViewShouldBeNullAfterWindowClosed()
        {
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            windowMediator.View.ShouldBeNull();
            windowMediator.ShowAsync(DataContext.Empty);
            windowMediator.View.ShouldEqual((TView)(object)view);
            windowMediator.CloseAsync(null);
            windowMediator.View.ShouldBeNull();
        }

        [TestMethod]
        public virtual void MediatorShouldCallOnNavigatedOnShow()
        {
            bool isInvoked = false;
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            NavigationDispatcher.OnNavigated = context =>
            {
                isInvoked = true;
                context.ShouldNotBeNull();
                context.NavigationMode.ShouldEqual(NavigationMode.New);
                context.NavigationType.ShouldEqual(NavigationType.Window);
                context.ViewModelTo.ShouldEqual(vm);
            };
            windowMediator.ShowAsync(DataContext.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void MediatorShouldCallOnNavigatingOnClose()
        {
            bool result = false;
            bool isInvoked = false;
            var view = new DialogViewMock();
            var vm = GetViewModel<NavigableViewModelMock>();
            ViewManager.GetViewDelegate = (model, s) => view;
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            NavigationDispatcher.OnNavigatingFromAsync = context =>
            {
                isInvoked = true;
                context.ShouldNotBeNull();
                context.NavigationMode.ShouldEqual(NavigationMode.Back);
                context.NavigationType.ShouldEqual(NavigationType.Window);
                context.ViewModelFrom.ShouldEqual(vm);
                return ToolkitExtensions.FromResult(result);
            };
            windowMediator.ShowAsync(DataContext.Empty);
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
            WindowViewMediatorBase<TView> windowMediator = Create(vm);
            NavigationDispatcher.OnNavigated = context =>
            {
                isInvoked = true;
                context.ShouldNotBeNull();
                context.NavigationMode.ShouldEqual(NavigationMode.Back);
                context.NavigationType.ShouldEqual(NavigationType.Window);
                context.ViewModelFrom.ShouldEqual(vm);
            };
            windowMediator.ShowAsync(DataContext.Empty);
            windowMediator.CloseAsync(null).Result.ShouldBeTrue();
            isInvoked.ShouldBeTrue();
        }

        protected WindowViewMediatorBase<TView> Create(IViewModel viewModel)
        {
            return Create(viewModel, ThreadManager, ViewManager, _wrapperManager, OperationCallbackManager, NavigationDispatcher);
        }

        protected abstract WindowViewMediatorBase<TView> Create(IViewModel viewModel, IThreadManager threadManager, IViewManager viewManager,
            IWrapperManager wrapperManager, IOperationCallbackManager callbackManager, INavigationDispatcher navigationDispatcher);

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            base.OnInit();
            ThreadManager.ImmediateInvokeOnUiThread = true;
            ThreadManager.ImmediateInvokeAsync = true;
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            _wrapperManager = new WrapperManager(ViewModelProvider);
        }

        #endregion
    }
}
