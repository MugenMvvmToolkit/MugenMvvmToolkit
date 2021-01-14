using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views
{
    public class ViewManagerTest : ComponentOwnerTestBase<ViewManager>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsInStateShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var owner = new ViewManager();
            var target = new object();
            var state = ViewLifecycleState.Appeared;

            owner.IsInState(target, state, DefaultMetadata).ShouldBeFalse();

            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i - 1 == componentCount;
                var component = new TestLifecycleTrackerComponent<ViewLifecycleState>(owner)
                {
                    IsInState = (o, t, s, m) =>
                    {
                        ++count;
                        t.ShouldEqual(target);
                        m.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                };
                owner.Components.TryAdd(component);
            }

            owner.IsInState(target, state, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var manager = new ViewManager();
            var invokeCount = 0;
            var state = "state";
            var view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, new TestViewModel());
            var lifecycleState = ViewLifecycleState.Initializing;
            for (var i = 0; i < count; i++)
            {
                var component = new TestViewLifecycleListener(manager)
                {
                    OnLifecycleChanged = (v, viewLifecycleState, st, metadata) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(view);
                        st.ShouldEqual(state);
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
                var view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, new TestViewModel());
                views.Add(view);
                var component = new TestViewProviderComponent(viewManager)
                {
                    TryGetViews = (r, context) =>
                    {
                        r.ShouldEqual(viewModel);
                        context.ShouldEqual(DefaultMetadata);
                        return new[] {view};
                    },
                    Priority = -i
                };
                viewManager.AddComponent(component);
            }

            viewManager.GetViews(viewModel, DefaultMetadata).AsList().ShouldEqual(views);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMappingsShouldBeHandledByComponents(int count)
        {
            var viewManager = new ViewManager();
            var mappings = new List<IViewMapping>();
            var view = new object();
            for (var i = 0; i < count; i++)
            {
                var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata);
                mappings.Add(mapping);
                var component = new TestViewMappingProviderComponent(viewManager)
                {
                    TryGetMappings = (r, context) =>
                    {
                        r.ShouldEqual(view);
                        context.ShouldEqual(DefaultMetadata);
                        return new[] {mapping};
                    },
                    Priority = -i
                };
                viewManager.AddComponent(component);
            }

            viewManager.GetMappings(view, DefaultMetadata).AsList().ShouldEqual(mappings);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task InitializeAsyncShouldBeHandledByComponents(int componentCount)
        {
            var manager = new ViewManager();
            var result = new ValueTask<IView?>(new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, new TestViewModel()));
            var cancellationToken = new CancellationTokenSource().Token;
            var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata);
            var viewModel = new TestViewModel();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestViewManagerComponent(manager)
                {
                    TryInitializeAsync = (viewMapping, r, meta, token) =>
                    {
                        ++invokeCount;
                        viewMapping.ShouldEqual(mapping);
                        r.ShouldEqual(viewModel);
                        meta.ShouldEqual(DefaultMetadata);
                        token.ShouldEqual(cancellationToken);
                        if (isLast)
                            return result;
                        return default;
                    },
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            (await manager.InitializeAsync(mapping, viewModel, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task CleanupAsyncShouldBeHandledByComponents(int componentCount)
        {
            var manager = new ViewManager();
            var result = true;
            var cancellationToken = new CancellationTokenSource().Token;
            var view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, new TestViewModel());
            var viewModel = new TestViewModel();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestViewManagerComponent(manager)
                {
                    TryCleanupAsync = (v, r, meta, token) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(view);
                        r.ShouldEqual(viewModel);
                        meta.ShouldEqual(DefaultMetadata);
                        token.ShouldEqual(cancellationToken);
                        if (isLast)
                            return new ValueTask<bool>(result);
                        return null;
                    },
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            var r = await manager.TryCleanupAsync(view, viewModel, cancellationToken, DefaultMetadata);
            r.ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override ViewManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new(collectionProvider);

        [Fact]
        public void InitializeAsyncShouldThrowNoComponents()
        {
            var viewManager = new ViewManager();
            ShouldThrow<InvalidOperationException>(() =>
            {
                var result = viewManager.InitializeAsync(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this).Result;
            });
        }
    }
}