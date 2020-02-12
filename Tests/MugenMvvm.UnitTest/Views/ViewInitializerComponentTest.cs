using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTest.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views
{
    public class ViewInitializerComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryInitializeAsyncShouldIgnoreNull()
        {
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var component = new ViewInitializerComponent();
            component.TryInitializeAsync(mapping, null, null, DefaultMetadata, CancellationToken.None).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldInitializeCleanupGetViews(int count)
        {
            var viewModel = new TestViewModel();
            var manager = new ViewManager();
            var component = new ViewInitializerComponent();
            manager.AddComponent(component);
            var results = new List<ViewInitializationResult>();

            for (int i = 0; i < count; i++)
            {
                var view = new object();
                var mapping = new ViewModelViewMapping("id" + i, typeof(object), typeof(TestViewModel), DefaultMetadata);
                results.Add(component.TryInitializeAsync(mapping, view, viewModel, DefaultMetadata, CancellationToken.None).Result);
            }

            for (int i = 0; i < count; i++)
            {
                component.TryGetViews(viewModel, DefaultMetadata).SequenceEqual(results.Select(result => result.View)).ShouldBeTrue();
                component.TryCleanupAsync(results[0].View, viewModel, DefaultMetadata, CancellationToken.None).IsCompleted.ShouldBeTrue();
                results.RemoveAt(0);
            }
            component.TryGetViews(viewModel, DefaultMetadata).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryCleanupAsyncShouldNotifyListeners(int count)
        {
            const int viewCount = 10;
            var viewModel = new TestViewModel();
            var manager = new ViewManager();
            var component = new ViewInitializerComponent();
            manager.AddComponent(component);
            var results = new List<ViewInitializationResult>();

            for (int i = 0; i < viewCount; i++)
            {
                var view = new object();
                var mapping = new ViewModelViewMapping("id" + i, typeof(object), typeof(TestViewModel), DefaultMetadata);
                results.Add(component.TryInitializeAsync(mapping, view, viewModel, DefaultMetadata, CancellationToken.None).Result);
            }

            int invokeCount = 0;
            IView? expectedView = null;
            for (int i = 0; i < count; i++)
            {
                var listener = new TestViewManagerListener
                {
                    OnViewInitialized = (viewManager, v, vm, meta) => throw new NotSupportedException(),
                    OnViewCleared = (viewManager, v, vm, meta) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(expectedView);
                        viewModel.ShouldEqual(viewModel);
                        meta.ShouldEqual(DefaultMetadata);
                    }
                };
                manager.AddComponent(listener);
            }

            for (int i = 0; i < viewCount; i++)
            {
                expectedView = results[0].View;
                component.TryCleanupAsync(results[0].View, viewModel, DefaultMetadata, CancellationToken.None).IsCompleted.ShouldBeTrue();
                results.RemoveAt(0);
            }

            invokeCount.ShouldEqual(count*viewCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryInitializeAsyncShouldInitializeViewAndNotifyListeners(int count)
        {
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new object();
            var clearView = view;
            var viewModel = new TestViewModel();
            var manager = new ViewManager();
            var component = new ViewInitializerComponent();
            manager.AddComponent(component);

            int invokeCount = 0;
            int clearInvokeCount = 0;
            for (int i = 0; i < count; i++)
            {
                var listener = new TestViewManagerListener
                {
                    OnViewInitialized = (viewManager, v, vm, meta) =>
                    {
                        ++invokeCount;
                        viewManager.ShouldEqual(manager);
                        v.Mapping.ShouldEqual(mapping);
                        v.View.ShouldEqual(view);
                        viewModel.ShouldEqual(viewModel);
                        meta.ShouldEqual(DefaultMetadata);
                    },
                    OnViewCleared = (viewManager, v, vm, meta) =>
                    {
                        ++clearInvokeCount;
                        v.Mapping.ShouldEqual(mapping);
                        v.View.ShouldEqual(clearView);
                        viewModel.ShouldEqual(viewModel);
                        meta.ShouldEqual(DefaultMetadata);
                    }
                };
                manager.AddComponent(listener);
            }

            var result = component.TryInitializeAsync(mapping, view, viewModel, DefaultMetadata, CancellationToken.None).Result;
            result.View.Mapping.ShouldEqual(mapping);
            result.View.View.ShouldEqual(view);
            result.ViewModel.ShouldEqual(viewModel);
            result.Metadata.ShouldEqual(DefaultMetadata);
            invokeCount.ShouldEqual(count);
            clearInvokeCount.ShouldEqual(0);

            invokeCount = 0;
            clearInvokeCount = 0;
            result = component.TryInitializeAsync(mapping, view, viewModel, DefaultMetadata, CancellationToken.None).Result;
            result.View.Mapping.ShouldEqual(mapping);
            result.View.View.ShouldEqual(view);
            result.ViewModel.ShouldEqual(viewModel);
            result.Metadata.ShouldEqual(DefaultMetadata);
            invokeCount.ShouldEqual(0);
            clearInvokeCount.ShouldEqual(0);

            invokeCount = 0;
            clearInvokeCount = 0;
            view = new object();
            result = component.TryInitializeAsync(mapping, view, viewModel, DefaultMetadata, CancellationToken.None).Result;
            result.View.Mapping.ShouldEqual(mapping);
            result.View.View.ShouldEqual(view);
            result.ViewModel.ShouldEqual(viewModel);
            result.Metadata.ShouldEqual(DefaultMetadata);
            invokeCount.ShouldEqual(count);
            clearInvokeCount.ShouldEqual(count);
        }

        #endregion
    }
}