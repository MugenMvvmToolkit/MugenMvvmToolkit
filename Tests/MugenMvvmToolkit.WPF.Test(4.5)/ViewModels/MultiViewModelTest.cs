using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestViewModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    public abstract class MultiViewModelTest<T> : TestBase
        where T : class, IMultiViewModel
    {
        #region Test methods

        [TestMethod]
        public void ItemsSourceShouldNotBeNull()
        {
            var multiViewModel = GetMultiViewModel();
            multiViewModel.ItemsSource.ShouldNotBeNull();
        }

        [TestMethod]
        public void AddViewModelMethodShouldAddVmToItemsSource()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();

            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);
            multiViewModel.ItemsSource.Count.ShouldEqual(1);
        }

        [TestMethod]
        public void AddViewModelMethodShouldNotThrowExceptionOnDuplicate()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.AddViewModel(viewModel);
        }

        [TestMethod]
        public void RemoveMethodShouldCallCloseMethodInVmFalse()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            viewModel.CloseDelegate = o => Empty.FalseTask;
            multiViewModel.RemoveViewModelAsync(viewModel).Result.ShouldBeFalse();
            multiViewModel.ItemsSource.ShouldContain(viewModel);
        }

        [TestMethod]
        public void RemoveMethodShouldCallOnNavigatingFromMethodInVmFalse()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            viewModel.OnNavigatingFromDelegate = o => Empty.FalseTask;
            multiViewModel.RemoveViewModelAsync(viewModel).Result.ShouldBeFalse();
            multiViewModel.ItemsSource.ShouldContain(viewModel);
        }

        [TestMethod]
        public void RemoveMethodShouldCallOnNavigatedFromMethod()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = GetMultiViewModel();
            ((SynchronizedNotifiableCollection<IViewModel>)multiViewModel.ItemsSource).ThreadManager = new ThreadManagerMock { IsUiThread = true };
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            INavigationContext ctx = null;
            viewModel.OnNavigatedFromDelegate = context => ctx = context;
            multiViewModel.RemoveViewModelAsync(viewModel).Result.ShouldBeTrue();
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
            ctx.NavigationMode.ShouldEqual(NavigationMode.Back);
        }

        [TestMethod]
        public void RemoveMethodShouldCallCloseMethodInVmTrue()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            viewModel.CloseDelegate = o => Empty.TrueTask;
            multiViewModel.RemoveViewModelAsync(viewModel).Result.ShouldBeTrue();
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
        }

        [TestMethod]
        public void RemoveMethodShouldCallOnNavigatingFromMethodInVmTrue()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            viewModel.OnNavigatingFromDelegate = o => Empty.TrueTask;
            multiViewModel.RemoveViewModelAsync(viewModel).Result.ShouldBeTrue();
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
        }

        [TestMethod]
        public void RemoveMethodShouldDisposeVmIfSetToTrue()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = (MultiViewModel)GetMultiViewModel();
            multiViewModel.DisposeViewModelOnRemove = true;
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            multiViewModel.RemoveViewModelAsync(viewModel);
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
            viewModel.IsDisposed.ShouldBeTrue();
        }

        [TestMethod]
        public void RemoveMethodShouldNotDisposeVmIfSetToFalse()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = (MultiViewModel)GetMultiViewModel();
            multiViewModel.DisposeViewModelOnRemove = false;
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            multiViewModel.RemoveViewModelAsync(viewModel);
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
            viewModel.IsDisposed.ShouldBeFalse();
        }

        [TestMethod]
        public void ClearShouldDisposeVmIfSetToTrue()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = (MultiViewModel)GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            multiViewModel.DisposeViewModelOnRemove = true;
            multiViewModel.Clear();
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
            multiViewModel.SelectedItem.ShouldBeNull();
            viewModel.IsDisposed.ShouldBeTrue();
        }


        [TestMethod]
        public void ClearShouldNotDisposeVmIfSetToFalse()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = (MultiViewModel)GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            multiViewModel.DisposeViewModelOnRemove = false;
            multiViewModel.Clear();
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
            multiViewModel.SelectedItem.ShouldBeNull();
            viewModel.IsDisposed.ShouldBeFalse();
        }

        [TestMethod]
        public void WhenVmWasAddedItShouldBeSelected()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            var viewModel1 = GetViewModel<NavigableViewModelMock>();
            var viewModel2 = GetViewModel<NavigableViewModelMock>();

            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel1);
            multiViewModel.AddViewModel(viewModel2);

            multiViewModel.SelectedItem.ShouldEqual(viewModel2);
            viewModel2.IsSelected.ShouldBeTrue();
            viewModel1.IsSelected.ShouldBeFalse();

            multiViewModel.RemoveViewModelAsync(viewModel2);
            multiViewModel.SelectedItem.ShouldEqual(viewModel1);
            viewModel2.IsSelected.ShouldBeFalse();
            viewModel1.IsSelected.ShouldBeTrue();
        }

        [TestMethod]
        public void WhenSelectedItemChangedVmShouldCallOnNavigatedFrom_ToMethods()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            var viewModel1 = GetViewModel<NavigableViewModelMock>();
            var viewModel2 = GetViewModel<NavigableViewModelMock>();

            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel1);
            multiViewModel.AddViewModel(viewModel2);

            multiViewModel.SelectedItem = null;
            INavigationContext ctxTo1 = null, ctxFrom1 = null, ctxTo2 = null, ctxFrom2 = null;
            viewModel1.OnNavigatedFromDelegate = context => ctxFrom1 = context;
            viewModel1.OnNavigatedToDelegate = context => ctxTo1 = context;
            viewModel2.OnNavigatedFromDelegate = context => ctxFrom2 = context;
            viewModel2.OnNavigatedToDelegate = context => ctxTo2 = context;

            multiViewModel.SelectedItem = viewModel1;
            ctxTo1.NavigationMode.ShouldEqual(NavigationMode.Refresh);
            ctxTo2.ShouldBeNull();
            ctxFrom1.ShouldBeNull();
            ctxFrom2.ShouldBeNull();

            ctxTo1 = null;
            multiViewModel.SelectedItem = viewModel2;
            ctxFrom1.NavigationMode.ShouldEqual(NavigationMode.Refresh);
            ctxTo2.NavigationMode.ShouldEqual(NavigationMode.Refresh);
            ctxTo1.ShouldBeNull();
            ctxFrom2.ShouldBeNull();
        }

        #endregion

        #region Methods

        protected T GetViewModel()
        {
            var viewModel = GetMultiViewModelInternal();
            InitializeViewModel(viewModel, IocContainer);
            return viewModel;
        }

        private IMultiViewModel GetMultiViewModel()
        {
            return GetViewModel();
        }

        protected abstract T GetMultiViewModelInternal();

        #endregion
    }
}
