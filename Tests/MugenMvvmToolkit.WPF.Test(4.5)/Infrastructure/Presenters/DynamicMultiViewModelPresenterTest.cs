#region Copyright

// ****************************************************************************
// <copyright file="DynamicMultiViewModelPresenterTest.cs">
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
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestViewModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Presenters
{
    [TestClass]
    public class DynamicMultiViewModelPresenterTest : TestBase
    {
        #region Methods

        [TestMethod]
        public void PresenterShouldAddVmToMultiViewModel()
        {
            var vm = GetViewModel<NavigableViewModelMock>();
            var viewModel = GetMultiViewModel();
            IDynamicViewModelPresenter presenter = new DynamicMultiViewModelPresenter(viewModel, OperationCallbackManager, (model, context, arg3) => true);
            var task = presenter.TryShowAsync(new DataContext(NavigationConstants.ViewModel.ToValue(vm)), null);
            task.ShouldNotBeNull();
            task.IsCompleted.ShouldBeFalse();
            viewModel.ItemsSource.Contains(vm).ShouldBeTrue();
        }

        [TestMethod]
        public void PresentShouldCloseViewModel()
        {
            bool isInvoked = false;
            var vm = GetViewModel<NavigableViewModelMock>();
            var viewModel = GetMultiViewModel();
            IDynamicViewModelPresenter presenter = new DynamicMultiViewModelPresenter(viewModel, OperationCallbackManager, (model, context, arg3) => true);
            var task = presenter.TryShowAsync(new DataContext(NavigationConstants.ViewModel.ToValue(vm)), null);
            task.ShouldNotBeNull();
            task.IsCompleted.ShouldBeFalse();
            viewModel.ItemsSource.Contains(vm).ShouldBeTrue();

            ViewModelPresenter.CloseAsync = (model, context) =>
            {
                isInvoked = true;
                var navigationContext = (INavigationContext)context;
                navigationContext.NavigationMode.ShouldEqual(NavigationMode.Remove);
                navigationContext.NavigationType.ShouldEqual(NavigationType.Tab);
                model.ShouldEqual(vm);
                return Empty.TrueTask;
            };
            presenter.TryCloseAsync(new DataContext(NavigationConstants.ViewModel.ToValue(vm)), null).Result.ShouldBeTrue();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void PreseterShouldUseDelegateToShowViewModel()
        {
            bool canShow = false;
            var vm = GetViewModel<NavigableViewModelMock>();
            var viewModel = GetMultiViewModel();
            IDynamicViewModelPresenter presenter = new DynamicMultiViewModelPresenter(viewModel,
                OperationCallbackManager, (model, context, arg3) => canShow);
            var task = presenter.TryShowAsync(new DataContext(NavigationConstants.ViewModel.ToValue(vm)), null);
            task.ShouldBeNull();
            viewModel.ItemsSource.Contains(vm).ShouldBeFalse();

            canShow = true;
            task = presenter.TryShowAsync(new DataContext(NavigationConstants.ViewModel.ToValue(vm)), null);
            task.ShouldNotBeNull();
            task.IsCompleted.ShouldBeFalse();
            viewModel.ItemsSource.Contains(vm).ShouldBeTrue();
        }

        #endregion

        #region Overrides of MultiViewModelTest

        protected MultiViewModel<IViewModel> GetMultiViewModel()
        {
            var vm = GetViewModel<MultiViewModel<IViewModel>>();
            return vm;
        }

        #endregion
    }
}
