using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.UnitTest.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views.Components
{
    public class ViewManagerComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryInitializeAsyncShouldIgnoreNull()
        {
            var mapping = new ViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var component = new ViewManagerComponent();
            component.TryInitializeAsync(mapping, new ViewModelViewRequest(), CancellationToken.None, DefaultMetadata).ShouldBeNull();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void ShouldInitializeCleanupGetViews(int count, bool owner)
        {
            var viewModel = owner ? new TestViewModelComponentOwner() : new TestViewModel();
            var manager = new ViewManager();
            var component = new ViewManagerComponent();
            manager.AddComponent(component);
            var results = new List<IView>();

            for (var i = 0; i < count; i++)
            {
                var view = new object();
                var mapping = new ViewMapping("id" + i, typeof(object), typeof(TestViewModel), DefaultMetadata);
                var result = component.TryInitializeAsync(mapping, new ViewModelViewRequest(viewModel, view), CancellationToken.None, DefaultMetadata)!.Result;
                results.Add(result);

                result.Mapping.ShouldEqual(mapping);
                result.ViewModel.ShouldEqual(viewModel);

                component.TryGetViews(viewModel, DefaultMetadata).AsList().ShouldContain(results);
                component.TryGetViews(view, DefaultMetadata).AsList().Single().ShouldEqual(results.Last());
            }

            for (var i = 0; i < count; i++)
            {
                var view = results[0];
                component.TryGetViews(viewModel, DefaultMetadata).AsList().ShouldContain(results);
                component.TryGetViews(view.Target, DefaultMetadata).AsList().Single().ShouldEqual(view);
                component.TryCleanupAsync(view, viewModel, CancellationToken.None, DefaultMetadata)!.IsCompleted.ShouldBeTrue();
                component.TryGetViews(view.Target, DefaultMetadata).AsList().ShouldBeEmpty();
                results.RemoveAt(0);
            }

            component.TryGetViews(viewModel, DefaultMetadata).AsList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void TryCleanupAsyncShouldNotifyViewLifecycle(int count, bool owner)
        {
            const int viewCount = 10;
            var viewModel = owner ? new TestViewModelComponentOwner() : new TestViewModel();
            var manager = new ViewManager();
            var component = new ViewManagerComponent();
            manager.AddComponent(component);
            var results = new List<IView>();

            for (var i = 0; i < viewCount; i++)
            {
                var view = new object();
                var mapping = new ViewMapping("id" + i, typeof(object), typeof(TestViewModel), DefaultMetadata);
                results.Add(component.TryInitializeAsync(mapping, new ViewModelViewRequest(viewModel, view), CancellationToken.None, DefaultMetadata)!.Result);
            }

            var states = new Dictionary<ViewLifecycleState, List<ViewLifecycleState>>();
            IView? expectedView = null;
            for (var i = 0; i < count; i++)
            {
                var listener = new TestViewLifecycleDispatcherComponent
                {
                    OnLifecycleChanged = (v, s, st, t, m) =>
                    {
                        if (!states.TryGetValue(s, out var list))
                        {
                            list = new List<ViewLifecycleState>();
                            states[s] = list;
                        }
                        list.Add(s);
                        v.ShouldEqual(expectedView);
                        st.ShouldEqual(viewModel);
                        t.ShouldEqual(typeof(TestViewModel));
                        m.ShouldEqual(DefaultMetadata);
                    }
                };
                manager.AddComponent(listener);
            }

            for (var i = 0; i < viewCount; i++)
            {
                expectedView = results[0];
                component.TryCleanupAsync(results[0], viewModel, CancellationToken.None, DefaultMetadata)!.IsCompleted.ShouldBeTrue();
                results.RemoveAt(0);
            }

            states.Count.ShouldEqual(2);
            states[ViewLifecycleState.Clearing].Count.ShouldEqual(count * viewCount);
            states[ViewLifecycleState.Cleared].Count.ShouldEqual(count * viewCount);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void TryInitializeAsyncShouldInitializeViewAndNotifyListeners(int count, bool owner)
        {
            var mapping = new ViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new object();
            var clearView = view;
            var viewModel = owner ? new TestViewModelComponentOwner() : new TestViewModel();
            var manager = new ViewManager();
            var component = new ViewManagerComponent();
            manager.AddComponent(component);

            var states = new Dictionary<ViewLifecycleState, List<ViewLifecycleState>>();
            for (var i = 0; i < count; i++)
            {
                var listener = new TestViewLifecycleDispatcherComponent
                {
                    OnLifecycleChanged = (vRaw, s, st, t, m) =>
                    {
                        var v = (IView)vRaw;
                        if (!states.TryGetValue(s, out var list))
                        {
                            list = new List<ViewLifecycleState>();
                            states[s] = list;
                        }
                        list.Add(s);
                        v.Mapping.ShouldEqual(mapping);
                        if (s == ViewLifecycleState.Clearing || s == ViewLifecycleState.Cleared)
                            v.Target.ShouldEqual(clearView);
                        else
                            v.Target.ShouldEqual(view);
                        m.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                };
                manager.AddComponent(listener);
            }

            var result = component.TryInitializeAsync(mapping, new ViewModelViewRequest(viewModel, view), CancellationToken.None, DefaultMetadata)!.Result;
            result.Mapping.ShouldEqual(mapping);
            result.Target.ShouldEqual(view);
            result.ViewModel.ShouldEqual(viewModel);
            states.Count.ShouldEqual(2);
            states[ViewLifecycleState.Initializing].Count.ShouldEqual(count);
            states[ViewLifecycleState.Initialized].Count.ShouldEqual(count);

            states.Clear();
            result = component.TryInitializeAsync(mapping, new ViewModelViewRequest(viewModel, view), CancellationToken.None, DefaultMetadata)!.Result;
            result.Mapping.ShouldEqual(mapping);
            result.Target.ShouldEqual(view);
            result.ViewModel.ShouldEqual(viewModel);
            states.Count.ShouldEqual(0);

            view = new object();
            result = component.TryInitializeAsync(mapping, new ViewModelViewRequest(viewModel, view), CancellationToken.None, DefaultMetadata)!.Result;
            result.Mapping.ShouldEqual(mapping);
            result.Target.ShouldEqual(view);
            result.ViewModel.ShouldEqual(viewModel);
            states[ViewLifecycleState.Initializing].Count.ShouldEqual(count);
            states[ViewLifecycleState.Initialized].Count.ShouldEqual(count);
            states[ViewLifecycleState.Clearing].Count.ShouldEqual(count);
            states[ViewLifecycleState.Cleared].Count.ShouldEqual(count);
        }

        #endregion
    }
}