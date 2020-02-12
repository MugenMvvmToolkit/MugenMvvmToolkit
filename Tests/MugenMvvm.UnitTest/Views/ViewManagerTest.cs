using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.ViewModels;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views
{
    public class ViewManagerTest : ComponentOwnerTestBase<ViewManager>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetViewsShouldBeHandledByComponents(int count)
        {
            var viewManager = new ViewManager();
            var views = new List<IView>();
            var viewModel = new TestViewModel();
            for (var i = 0; i < count; i++)
            {
                var view = new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this);
                views.Add(view);
                var component = new TestViewProviderComponent
                {
                    TryGetViews = (vm, context) =>
                    {
                        vm.ShouldEqual(viewModel);
                        context.ShouldEqual(DefaultMetadata);
                        return new[] { view };
                    },
                    Priority = -i
                };
                viewManager.AddComponent(component);
            }

            viewManager.GetViews(viewModel, DefaultMetadata).SequenceEqual(views).ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMappingByViewShouldBeHandledByComponents(int count)
        {
            var viewManager = new ViewManager();
            var mappings = new List<IViewModelViewMapping>();
            var view = new object();
            for (var i = 0; i < count; i++)
            {
                var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
                mappings.Add(mapping);
                var component = new TestViewModelViewMappingProviderComponent
                {
                    TryGetMappingByView = (v, context) =>
                    {
                        v.ShouldEqual(view);
                        context.ShouldEqual(DefaultMetadata);
                        return new[] { mapping };
                    },
                    Priority = -i
                };
                viewManager.AddComponent(component);
            }

            viewManager.GetMappingByView(view, DefaultMetadata).SequenceEqual(mappings).ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMappingByViewModelShouldBeHandledByComponents(int count)
        {
            var viewManager = new ViewManager();
            var mappings = new List<IViewModelViewMapping>();
            var viewModel = new TestViewModel();
            for (var i = 0; i < count; i++)
            {
                var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
                mappings.Add(mapping);
                var component = new TestViewModelViewMappingProviderComponent
                {
                    TryGetMappingByViewModel = (vm, context) =>
                    {
                        vm.ShouldEqual(viewModel);
                        context.ShouldEqual(DefaultMetadata);
                        return new[] { mapping };
                    },
                    Priority = -i
                };
                viewManager.AddComponent(component);
            }

            viewManager.GetMappingByViewModel(viewModel, DefaultMetadata).SequenceEqual(mappings).ShouldBeTrue();
        }

        [Fact]
        public void InitializeAsyncShouldThrowNoComponents()
        {
            var viewManager = new ViewManager();
            ShouldThrow<InvalidOperationException>(() => viewManager.InitializeAsync(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, null));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void InitializeAsyncShouldBeHandledByComponents(int componentCount)
        {
            var manager = new ViewManager();
            var result = Task.FromResult(new ViewInitializationResult());
            var cancellationToken = new CancellationTokenSource().Token;
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new object();
            var viewModel = new TestViewModel();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestViewInitializerComponent()
                {
                    TryInitializeAsync = (viewMapping, v, vm, meta, token) =>
                    {
                        ++invokeCount;
                        viewMapping.ShouldEqual(mapping);
                        v.ShouldEqual(view);
                        vm.ShouldEqual(viewModel);
                        meta.ShouldEqual(DefaultMetadata);
                        token.ShouldEqual(cancellationToken);
                        if (isLast)
                            return result;
                        return null;
                    },
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            manager.InitializeAsync(mapping, view, viewModel, DefaultMetadata, cancellationToken).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CleanupAsyncShouldBeHandledByComponents(int componentCount)
        {
            var manager = new ViewManager();
            var result = Task.FromResult(new ViewInitializationResult());
            var cancellationToken = new CancellationTokenSource().Token;
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new View(mapping, new object());
            var viewModel = new TestViewModel();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestViewInitializerComponent()
                {
                    TryCleanupAsync = (v, vm, meta, token) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(view);
                        vm.ShouldEqual(viewModel);
                        meta.ShouldEqual(DefaultMetadata);
                        token.ShouldEqual(cancellationToken);
                        if (isLast)
                            return result;
                        return null;
                    },
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            manager.CleanupAsync(view, viewModel, DefaultMetadata, cancellationToken).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Fact]
        public void CleanupAsyncShouldThrowNoComponents()
        {
            var viewManager = new ViewManager();
            var view = new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this);
            ShouldThrow<InvalidOperationException>(() => viewManager.CleanupAsync(view, new TestViewModel()));
        }

        protected override ViewManager GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ViewManager(collectionProvider);
        }

        #endregion
    }
}