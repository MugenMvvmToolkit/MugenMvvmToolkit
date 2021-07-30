﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
using MugenMvvm.UnitTests.Components;
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
        public async Task CleanupAsyncShouldBeHandledByComponents(int componentCount)
        {
            var result = true;
            var view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), this, new TestViewModel());
            var viewModel = new TestViewModel();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestViewManagerComponent
                {
                    TryCleanupAsync = (m, v, r, meta, token) =>
                    {
                        ++invokeCount;
                        m.ShouldEqual(ViewManager);
                        v.ShouldEqual(view);
                        r.ShouldEqual(viewModel);
                        meta.ShouldEqual(Metadata);
                        token.ShouldEqual(DefaultCancellationToken);
                        if (isLast)
                            return Task.FromResult(result);
                        return Default.FalseTask;
                    },
                    Priority = -i
                };
                ViewManager.AddComponent(component);
            }

            var r = await ViewManager.TryCleanupAsync(view, viewModel, DefaultCancellationToken, Metadata);
            r.ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMappingsShouldBeHandledByComponents(int count)
        {
            var mappings = new List<IViewMapping>();
            var view = new object();
            for (var i = 0; i < count; i++)
            {
                var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata);
                mappings.Add(mapping);
                ViewManager.AddComponent(new TestViewMappingProviderComponent
                {
                    TryGetMappings = (m, r, context) =>
                    {
                        m.ShouldEqual(ViewManager);
                        r.ShouldEqual(view);
                        context.ShouldEqual(Metadata);
                        return new[] { mapping };
                    },
                    Priority = -i
                });
            }

            ViewManager.GetMappings(view, Metadata).ShouldEqual(mappings);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetViewsShouldBeHandledByComponents(int count)
        {
            var views = new List<IView>();
            var viewModel = new TestViewModel();
            for (var i = 0; i < count; i++)
            {
                var view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), this, new TestViewModel());
                views.Add(view);
                ViewManager.AddComponent(new TestViewProviderComponent
                {
                    TryGetViews = (m, r, context) =>
                    {
                        m.ShouldEqual(ViewManager);
                        r.ShouldEqual(viewModel);
                        context.ShouldEqual(Metadata);
                        return new[] { view };
                    },
                    Priority = -i
                });
            }

            ViewManager.GetViews(viewModel, Metadata).ShouldEqual(views);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task InitializeAsyncShouldBeHandledByComponents(int componentCount)
        {
            var result = new ValueTask<IView?>(new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), this, new TestViewModel()));
            var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata);
            var viewModel = new TestViewModel();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                ViewManager.AddComponent(new TestViewManagerComponent
                {
                    TryInitializeAsync = (m, viewMapping, r, meta, token) =>
                    {
                        ++invokeCount;
                        m.ShouldEqual(ViewManager);
                        viewMapping.ShouldEqual(mapping);
                        r.ShouldEqual(viewModel);
                        meta.ShouldEqual(Metadata);
                        token.ShouldEqual(DefaultCancellationToken);
                        if (isLast)
                            return result;
                        return default;
                    },
                    Priority = -i
                });
            }

            (await ViewManager.InitializeAsync(mapping, viewModel, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Fact]
        public void InitializeAsyncShouldThrowNoComponents() =>
            ShouldThrow<InvalidOperationException>(() =>
            {
                var result = ViewManager.InitializeAsync(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), this).Result;
            });

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsInStateShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var target = new object();
            var state = ViewLifecycleState.Appeared;

            ViewManager.IsInState(target, state, Metadata).ShouldBeFalse();

            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i - 1 == componentCount;
                ViewManager.Components.TryAdd(new TestLifecycleTrackerComponent<IViewManager, ViewLifecycleState>
                {
                    IsInState = (o, t, s, m) =>
                    {
                        ++count;
                        o.ShouldEqual(ViewManager);
                        t.ShouldEqual(target);
                        m.ShouldEqual(Metadata);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                });
            }

            ViewManager.IsInState(target, state, Metadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var state = "state";
            var view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), this, new TestViewModel());
            var lifecycleState = ViewLifecycleState.Initializing;
            for (var i = 0; i < count; i++)
            {
                ViewManager.AddComponent(new TestViewLifecycleListener
                {
                    OnLifecycleChanged = (m, v, viewLifecycleState, st, metadata) =>
                    {
                        ++invokeCount;
                        m.ShouldEqual(ViewManager);
                        v.View.ShouldEqual(view);
                        st.ShouldEqual(state);
                        viewLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(Metadata);
                    },
                    Priority = i
                });
            }

            ViewManager.OnLifecycleChanged(view, lifecycleState, state, Metadata);
            invokeCount.ShouldEqual(count);
        }

        protected override IViewManager GetViewManager() => GetComponentOwner(ComponentCollectionManager);

        protected override ViewManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}