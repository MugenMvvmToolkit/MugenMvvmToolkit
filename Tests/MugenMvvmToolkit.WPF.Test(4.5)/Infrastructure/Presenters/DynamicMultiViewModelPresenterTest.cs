using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
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
            IDynamicViewModelPresenter presenter = new DynamicMultiViewModelPresenter(viewModel,
                OperationCallbackManager, (model, context, arg3) => true);
            var task = presenter.TryShowAsync(vm, DataContext.Empty, null);
            task.ShouldNotBeNull();
            task.IsCompleted.ShouldBeFalse();
            viewModel.ItemsSource.Contains(vm).ShouldBeTrue();
        }

        [TestMethod]
        public void PresentShouldInvokeCallbackOnRemove()
        {
            var vm = GetViewModel<NavigableViewModelMock>();
            IOperationCallback operationCallback = null;
            OperationCallbackManager.Register = (type, o, arg3, arg4) =>
            {
                type.ShouldEqual(OperationType.TabNavigation);
                operationCallback = arg3;
            };
            OperationCallbackManager.SetResult = (o, result) =>
            {
                result.Operation.ShouldEqual(OperationType.TabNavigation);
                o.ShouldEqual(vm);
                operationCallback.Invoke(result);
            };

            vm.OperationResult = true;
            var viewModel = GetMultiViewModel();
            ((SynchronizedNotifiableCollection<IViewModel>)viewModel.ItemsSource).ThreadManager = new ThreadManagerMock { IsUiThread = true };
            IDynamicViewModelPresenter presenter = new DynamicMultiViewModelPresenter(viewModel,
                OperationCallbackManager, (model, context, arg3) => true);
            var task = presenter.TryShowAsync(vm, DataContext.Empty, null);
            task.ShouldNotBeNull();
            task.IsCompleted.ShouldBeFalse();
            viewModel.RemoveViewModelAsync(vm).Result.ShouldBeTrue();
            task.IsCompleted.ShouldBeTrue();
            task.Result.Result.ShouldEqual(true);
            operationCallback.ShouldNotBeNull();
        }

        [TestMethod]
        public void PresentShouldInvokeCallbackOnClear()
        {
            var vm = GetViewModel<NavigableViewModelMock>();
            IOperationCallback operationCallback = null;
            OperationCallbackManager.Register = (type, o, arg3, arg4) =>
            {
                type.ShouldEqual(OperationType.TabNavigation);
                operationCallback = arg3;
            };
            OperationCallbackManager.SetResult = (o, result) =>
            {
                result.Operation.ShouldEqual(OperationType.TabNavigation);
                o.ShouldEqual(vm);
                operationCallback.Invoke(result);
            };

            vm.OperationResult = true;
            var viewModel = GetMultiViewModel();
            IDynamicViewModelPresenter presenter = new DynamicMultiViewModelPresenter(viewModel,
                OperationCallbackManager);
            var task = presenter.TryShowAsync(vm, DataContext.Empty, null);
            task.ShouldNotBeNull();
            task.IsCompleted.ShouldBeFalse();
            viewModel.Clear();
            task.IsCompleted.ShouldBeTrue();
            task.Result.Result.ShouldEqual(true);
            operationCallback.ShouldNotBeNull();
        }

        [TestMethod]
        public void PreseterShouldUseDelegateToShowViewModel()
        {
            bool canShow = false;
            var vm = GetViewModel<NavigableViewModelMock>();
            var viewModel = GetMultiViewModel();
            IDynamicViewModelPresenter presenter = new DynamicMultiViewModelPresenter(viewModel,
                OperationCallbackManager, (model, context, arg3) => canShow);
            var task = presenter.TryShowAsync(vm, DataContext.Empty, null);
            task.ShouldBeNull();
            viewModel.ItemsSource.Contains(vm).ShouldBeFalse();

            canShow = true;
            task = presenter.TryShowAsync(vm, DataContext.Empty, null);
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
