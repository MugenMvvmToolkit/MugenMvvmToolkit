#region Copyright

// ****************************************************************************
// <copyright file="MultiViewModelTest.cs">
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
        where T : class, IMultiViewModel<IViewModel>
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
        public void InsertViewModelMethodShouldInsertVmToItemsSource()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();

            var multiViewModel = GetMultiViewModel();
            multiViewModel.InsertViewModel(0, viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);
            multiViewModel.ItemsSource.Count.ShouldEqual(1);

            viewModel = GetViewModel<NavigableViewModelMock>();
            multiViewModel.InsertViewModel(0, viewModel);
            multiViewModel.ItemsSource[0].ShouldEqual(viewModel);
            multiViewModel.ItemsSource.Count.ShouldEqual(2);
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
        public void RemoveMethodShouldCallCloseMethodFalse()
        {
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            ViewModelPresenter.CloseAsync = (vm, ctx) => Empty.FalseTask;
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
            NavigationDispatcher.OnNavigated = context => ctx = context;
            ViewModelPresenter.CloseAsync = (vm, c) => Empty.TrueTask;
            multiViewModel.RemoveViewModelAsync(viewModel).Result.ShouldBeTrue();
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
            ctx.NavigationMode.ShouldEqual(NavigationMode.Remove);
        }

        [TestMethod]
        public void RemoveMethodShouldCallCloseMethodInVmTrue()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            ViewModelPresenter.CloseAsync = (vm, ctx) => Empty.TrueTask;
            multiViewModel.RemoveViewModelAsync(viewModel).Result.ShouldBeTrue();
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
        }

        [TestMethod]
        public void RemoveMethodShouldDisposeVmIfSetToTrue()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            ViewModelPresenter.CloseAsync = (vm, ctx) => Empty.TrueTask;
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = (MultiViewModel<IViewModel>)GetMultiViewModel();
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
            ViewModelPresenter.CloseAsync = (vm, ctx) => Empty.TrueTask;
            var viewModel = GetViewModel<NavigableViewModelMock>();
            var multiViewModel = (MultiViewModel<IViewModel>)GetMultiViewModel();
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
            var multiViewModel = (MultiViewModel<IViewModel>)GetMultiViewModel();
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
            var multiViewModel = (MultiViewModel<IViewModel>)GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.ShouldContain(viewModel);

            multiViewModel.DisposeViewModelOnRemove = false;
            multiViewModel.Clear();
            multiViewModel.ItemsSource.ShouldNotContain(viewModel);
            multiViewModel.SelectedItem.ShouldBeNull();
            viewModel.IsDisposed.ShouldBeFalse();
        }

        [TestMethod]
        public void SelectedItemChangedEvent()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            var viewModel1 = GetViewModel<NavigableViewModelMock>();
            var viewModel2 = GetViewModel<NavigableViewModelMock>();

            var multiViewModel = GetMultiViewModel();
            multiViewModel.AddViewModel(viewModel1);
            multiViewModel.AddViewModel(viewModel2);

            multiViewModel.SelectedItem = viewModel1;
            int isGenericInvoked = 0;
            int isNonGenericInvoked = 0;
            ((IMultiViewModel)multiViewModel).SelectedItemChanged += (sender, args) =>
           {
               sender.ShouldEqual(multiViewModel);
               args.OldValue.ShouldEqual(viewModel1);
               args.NewValue.ShouldEqual(viewModel2);
               isNonGenericInvoked++;
           };
            multiViewModel.SelectedItemChanged += (sender, args) =>
            {
                sender.ShouldEqual(multiViewModel);
                args.OldValue.ShouldEqual(viewModel1);
                args.NewValue.ShouldEqual(viewModel2);
                isGenericInvoked++;
            };
            multiViewModel.SelectedItem = viewModel2;
            isGenericInvoked.ShouldEqual(1);
            isNonGenericInvoked.ShouldEqual(1);
        }

        [TestMethod]
        public void ViewModelAddedEvent()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            ViewModelPresenter.CloseAsync = (vm, ctx) => Empty.TrueTask;
            int isGenericInvoked = 0;
            int isNonGenericInvoked = 0;
            var viewModel = GetViewModel<NavigableViewModelMock>();

            var multiViewModel = GetMultiViewModel();
            ((IMultiViewModel)multiViewModel).ViewModelAdded += (sender, args) =>
            {
                sender.ShouldEqual(multiViewModel);
                args.Value.ShouldEqual(viewModel);
                isNonGenericInvoked++;
            };
            multiViewModel.ViewModelAdded += (sender, args) =>
            {
                sender.ShouldEqual(multiViewModel);
                args.Value.ShouldEqual(viewModel);
                isGenericInvoked++;
            };
            multiViewModel.AddViewModel(viewModel);
            isGenericInvoked.ShouldEqual(1);
            isNonGenericInvoked.ShouldEqual(1);

            isGenericInvoked = 0;
            isNonGenericInvoked = 0;
            multiViewModel.RemoveViewModelAsync(viewModel);
            multiViewModel.InsertViewModel(0, viewModel);
            isGenericInvoked.ShouldEqual(1);
            isNonGenericInvoked.ShouldEqual(1);

            isGenericInvoked = 0;
            isNonGenericInvoked = 0;
            multiViewModel.RemoveViewModelAsync(viewModel);
            multiViewModel.ItemsSource.Add(viewModel);
            isGenericInvoked.ShouldEqual(1);
            isNonGenericInvoked.ShouldEqual(1);
        }

        [TestMethod]
        public void ViewModelRemovedEvent()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            ViewModelPresenter.CloseAsync = (vm, ctx) => Empty.TrueTask;
            int isGenericInvoked = 0;
            int isNonGenericInvoked = 0;
            var viewModel = GetViewModel<NavigableViewModelMock>();

            var multiViewModel = GetMultiViewModel();
            ((IMultiViewModel)multiViewModel).ViewModelRemoved += (sender, args) =>
            {
                sender.ShouldEqual(multiViewModel);
                args.Value.ShouldEqual(viewModel);
                isNonGenericInvoked++;
            };
            multiViewModel.ViewModelRemoved += (sender, args) =>
            {
                sender.ShouldEqual(multiViewModel);
                args.Value.ShouldEqual(viewModel);
                isGenericInvoked++;
            };
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.RemoveViewModelAsync(viewModel);
            isGenericInvoked.ShouldEqual(1);
            isNonGenericInvoked.ShouldEqual(1);

            isGenericInvoked = 0;
            isNonGenericInvoked = 0;
            multiViewModel.AddViewModel(viewModel);
            multiViewModel.ItemsSource.Remove(viewModel);
            isGenericInvoked.ShouldEqual(1);
            isNonGenericInvoked.ShouldEqual(1);
        }

        [TestMethod]
        public void WhenVmWasAddedItShouldBeSelected()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            ViewModelPresenter.CloseAsync = (vm, ctx) => Empty.TrueTask;
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
        public void WhenSelectedItemChangedVmShouldCallOnNavigatedMethod()
        {
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            ViewModelPresenter.CloseAsync = (vm, ctx) => Empty.TrueTask;
            NavigationDispatcher.OnNavigated = context =>
            {
                (context.ViewModelFrom as INavigableViewModel)?.OnNavigatedFrom(context);
                (context.ViewModelTo as INavigableViewModel)?.OnNavigatedTo(context);
            };
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

        private IMultiViewModel<IViewModel> GetMultiViewModel()
        {
            return GetViewModel();
        }

        protected abstract T GetMultiViewModelInternal();

        #endregion
    }
}
