using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.Test.TestViewModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public class GridViewModelTest : TestBase
    {
        #region Test methods

        [TestMethod]
        public void ItemsSourceShouldBeEmptyWhenVmCreated()
        {
            var gridViewModel = new GridViewModel<object>();
            gridViewModel.ItemsSource.ShouldBeEmpty();
        }

        [TestMethod]
        public void ItemsSourceChangedGenericShouldBeInvokedWhenItemsSourceChanged()
        {
            ThreadManager.IsUiThread = true;
            bool isItemsSourceChanged = false;
            var model = new GridModel { Name = "test" };
            var listItems = new List<GridModel> { model };

            var gridViewModel = GetViewModel<GridViewModel<GridModel>>();
            gridViewModel.ItemsSourceChanged += (sender, data) =>
                                                {
                                                    if (isItemsSourceChanged)
                                                        throw new InvalidOperationException();
                                                    isItemsSourceChanged = true;
                                                    gridViewModel.ItemsSource.ShouldEqual(data.Value);
                                                };
            gridViewModel.UpdateItemsSource(listItems);
            isItemsSourceChanged.ShouldBeTrue();
        }

        [TestMethod]
        public void ItemsSourceChangedNonGenericShouldBeInvokedWhenItemsSourceChanged()
        {
            ThreadManager.IsUiThread = true;
            bool isItemsSourceChanged = false;
            var model = new GridModel { Name = "test" };
            var listItems = new List<GridModel> { model };

            IGridViewModel gridViewModel = GetViewModel<GridViewModel<GridModel>>();
            gridViewModel.ItemsSourceChanged += (sender, data) =>
            {
                if (isItemsSourceChanged)
                    throw new InvalidOperationException();
                isItemsSourceChanged = true;
                gridViewModel.ItemsSource.ShouldEqual(data.Value);
            };
            gridViewModel.UpdateItemsSource(listItems);
            isItemsSourceChanged.ShouldBeTrue();
        }

        [TestMethod]
        public void SelectedItemChangedGenericShouldBeInvokedWhenSelectedItemChanged()
        {
            ThreadManager.IsUiThread = true;
            bool isFocusedRowChanged = false;
            var model = new GridModel { Name = "test" };
            var listItems = new List<GridModel> { model };

            var gridViewModel = GetViewModel<GridViewModel<GridModel>>();
            gridViewModel.SelectedItemChanged += (sender, args) =>
                                                 {
                                                     if (isFocusedRowChanged)
                                                         throw new InvalidOperationException();
                                                     isFocusedRowChanged = true;
                                                     args.OldValue.ShouldBeNull();
                                                     args.NewValue.ShouldEqual(model);
                                                 };
            gridViewModel.UpdateItemsSource(listItems);
            gridViewModel.SelectedItem = model;
            isFocusedRowChanged.ShouldBeTrue();
        }

        [TestMethod]
        public void SelectedItemChangedNonGenericShouldBeInvokedWhenSelectedItemChanged()
        {
            ThreadManager.IsUiThread = true;
            bool isFocusedRowChanged = false;
            var model = new GridModel { Name = "test" };
            var listItems = new List<GridModel> { model };

            IGridViewModel gridViewModel = GetViewModel<GridViewModel<GridModel>>();
            gridViewModel.SelectedItemChanged += (sender, args) =>
            {
                if (isFocusedRowChanged)
                    throw new InvalidOperationException();
                isFocusedRowChanged = true;
                args.OldValue.ShouldBeNull();
                args.NewValue.ShouldEqual(model);
            };
            gridViewModel.UpdateItemsSource(listItems);
            gridViewModel.SelectedItem = model;
            isFocusedRowChanged.ShouldBeTrue();
        }

        [TestMethod]
        public void WhenItemsSourceChangedSelectedItemShouldBeSetToNullTest()
        {
            var model = new GridModel { Name = "test" };
            var listItems = new List<GridModel> { model };
            var gridViewModel = GetViewModel<GridViewModel<GridModel>>();

            gridViewModel.UpdateItemsSource(listItems);
            gridViewModel.SelectedItem = model;

            gridViewModel.SelectedItem.ShouldEqual(model);
            gridViewModel.UpdateItemsSource(null);
            gridViewModel.SelectedItem.ShouldBeNull();
        }

        [TestMethod]
        public void FilterShouldFilterRecords()
        {
            var model = new GridModel { Name = "test" };
            var model1 = new GridModel { Name = "test1" };
            var listItems = new List<GridModel> { model, model1 };

            var gridViewModel = GetViewModel<GridViewModel<GridModel>>();

            gridViewModel.Filter = item => item.Name == string.Empty;
            gridViewModel.UpdateItemsSource(listItems);
            gridViewModel.ItemsSource.Contains(model).ShouldBeFalse();
            gridViewModel.ItemsSource.Contains(model1).ShouldBeFalse();

            gridViewModel.Filter = item => true;
            gridViewModel.ItemsSource.Contains(model).ShouldBeTrue();
            gridViewModel.ItemsSource.Contains(model1).ShouldBeTrue();

            gridViewModel.Filter = item => item.Name == "test";
            gridViewModel.ItemsSource.Contains(model).ShouldBeTrue();
            gridViewModel.ItemsSource.Contains(model1).ShouldBeFalse();
        }

        [TestMethod]
        public void InternalSelectedRowEventTest()
        {
            var model = new GridModel { Name = "test" };
            var listItems = new List<GridModel> { model };
            var viewModel = GetViewModel<GridViewModelMock>();
            viewModel.ItemsSourceChangingResult = listItems;

            viewModel.UpdateItemsSource(listItems);
            viewModel.SelectedItem = model;
            viewModel.SelectedItemChanging.ShouldEqual(model);
            viewModel.SelectedItem.ShouldBeNull();

            viewModel.SelectedItemChangingResult = model;
            viewModel.SelectedItem = model;
            viewModel.SelectedItem.ShouldEqual(model);

            viewModel.SelectedItemChangedOld.ShouldBeNull();
            viewModel.SelectedItemChangedNew.ShouldEqual(model);
        }

        [TestMethod]
        public void InternalItemsSourceEventsTest()
        {
            var model = new GridModel { Name = "test" };
            var listItems = new List<GridModel> { model };
            var viewModel = GetViewModel<GridViewModelMock>();

            viewModel.UpdateItemsSource(listItems);
            viewModel.ItemsSourceChangingValue.ShouldEqual(listItems);
            viewModel.ItemsSource.ShouldBeEmpty();

            viewModel.ItemsSourceChangingResult = listItems;
            viewModel.UpdateItemsSource(listItems);
            viewModel.ItemsSourceChangingValue.ShouldEqual(listItems);
            viewModel.ItemsSourceChangedValue.ShouldEqual(viewModel.ItemsSource);
            viewModel.ItemsSource.ShouldNotBeNull();
            viewModel.ItemsSource.All(listItems.Contains).ShouldBeTrue();
        }

        [TestMethod]
        public void SetOriginalItemsSourceShouldGetDataFromPrevious()
        {
            var model = new GridModel { Name = "test" };
            var listItems = new List<GridModel> { model };
            var viewModel = GetViewModel<GridViewModel<GridModel>>();
            viewModel.UpdateItemsSource(listItems);

            var newItemsSource = new ObservableCollection<GridModel>();
            viewModel.SetOriginalItemsSource(newItemsSource);
            newItemsSource.SequenceEqual(listItems).ShouldBeTrue();
        }

        #endregion
    }
}
