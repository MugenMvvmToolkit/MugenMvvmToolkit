using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.UnitTest.Views.Internal;
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
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var manager = new ViewManager();
            var invokeCount = 0;
            var state = "state";
            var view = new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, new TestViewModel());
            var lifecycleState = ViewLifecycleState.Initializing;
            for (var i = 0; i < count; i++)
            {
                var component = new TestViewLifecycleDispatcherComponent
                {
                    OnLifecycleChanged = (v, viewLifecycleState, st, stateType, metadata) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(view);
                        st.ShouldEqual(state);
                        stateType.ShouldEqual(state.GetType());
                        viewLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                };
                manager.AddComponent(component);
            }

            manager.OnLifecycleChanged(view, lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

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
                var view = new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, new TestViewModel());
                views.Add(view);
                var component = new TestViewProviderComponent
                {
                    TryGetViews = (r, t, context) =>
                    {
                        r.ShouldEqual(viewModel);
                        t.ShouldEqual(typeof(TestViewModel));
                        context.ShouldEqual(DefaultMetadata);
                        return new[] {view};
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
        public void GetMappingsShouldBeHandledByComponents(int count)
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
                    TryGetMappings = (r, t, context) =>
                    {
                        r.ShouldEqual(view);
                        t.ShouldEqual(typeof(object));
                        context.ShouldEqual(DefaultMetadata);
                        return new[] {mapping};
                    },
                    Priority = -i
                };
                viewManager.AddComponent(component);
            }

            viewManager.GetMappings(view, DefaultMetadata).SequenceEqual(mappings).ShouldBeTrue();
        }

        [Fact]
        public void InitializeAsyncShouldThrowNoComponents()
        {
            var viewManager = new ViewManager();
            ShouldThrow<InvalidOperationException>(() => viewManager.InitializeAsync(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void InitializeAsyncShouldBeHandledByComponents(int componentCount)
        {
            var manager = new ViewManager();
            var result = Task.FromResult<IView>(new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, new TestViewModel()));
            var cancellationToken = new CancellationTokenSource().Token;
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var viewModel = new TestViewModel();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestViewInitializerComponent
                {
                    TryInitializeAsync = (viewMapping, r, t, meta, token) =>
                    {
                        ++invokeCount;
                        viewMapping.ShouldEqual(mapping);
                        r.ShouldEqual(viewModel);
                        t.ShouldEqual(typeof(TestViewModel));
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

            manager.InitializeAsync(mapping, viewModel, cancellationToken, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CleanupAsyncShouldBeHandledByComponents(int componentCount)
        {
            var manager = new ViewManager();
            var result = Task.FromResult(this);
            var cancellationToken = new CancellationTokenSource().Token;
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, new TestViewModel());
            var viewModel = new TestViewModel();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestViewInitializerComponent
                {
                    TryCleanupAsync = (v, r, t, meta, token) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(view);
                        r.ShouldEqual(viewModel);
                        t.ShouldEqual(typeof(TestViewModel));
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

            manager.CleanupAsync(view, viewModel, cancellationToken, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override ViewManager GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ViewManager(collectionProvider);
        }

        #endregion
    }
}