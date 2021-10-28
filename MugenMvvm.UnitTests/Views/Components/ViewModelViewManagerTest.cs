using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewModelViewManagerTest : UnitTestBase
    {
        private readonly View _view;
        private readonly TestViewModel _viewModel;

        public ViewModelViewManagerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata);
            _view = new View(mapping, new object(), _viewModel);
            ViewManager.AddComponent(new ViewModelViewManager(AttachedValueManager, ViewModelManager, ComponentCollectionManager));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ShouldInitializeCleanupGetViews(int count)
        {
            var results = new List<IView>();
            for (var i = 0; i < count; i++)
            {
                var view = new object();
                var mapping = new ViewMapping("id" + i, typeof(TestViewModel), typeof(object), Metadata);
                var result = (await ViewManager.TryInitializeAsync(mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, Metadata))!;
                results.Add(result);

                result.Mapping.ShouldEqual(mapping);
                result.ViewModel.ShouldEqual(_viewModel);

                ViewManager.GetViews(_viewModel, Metadata).ShouldContain(results);
                ViewManager.GetViews(view, Metadata).Single().ShouldEqual(results.Last());
            }

            for (var i = 0; i < count; i++)
            {
                var view = results[0];
                ViewManager.GetViews(_viewModel, Metadata).ShouldContain(results);
                ViewManager.GetViews(view.Target, Metadata).Single().ShouldEqual(view);
                await ViewManager.TryCleanupAsync(view, _viewModel, CancellationToken.None, Metadata);
                ViewManager.GetViews(view.Target, Metadata).ShouldBeEmpty();
                results.RemoveAt(0);
            }

            ViewManager.GetViews(_viewModel, Metadata).ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task TryCleanupAsyncShouldNotifyListeners(int count)
        {
            const int viewCount = 10;
            var results = new List<IView>();

            for (var i = 0; i < viewCount; i++)
            {
                var view = new object();
                var mapping = new ViewMapping("id" + i, typeof(TestViewModel), typeof(object), Metadata);
                results.Add((await ViewManager.TryInitializeAsync(mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, Metadata))!);
            }

            var states = new Dictionary<ViewLifecycleState, List<ViewLifecycleState>>();
            IView? expectedView = null;
            for (var i = 0; i < count; i++)
            {
                var listener = new TestViewLifecycleListener
                {
                    OnLifecycleChanged = (vm, v, s, st, m) =>
                    {
                        vm.ShouldEqual(ViewManager);
                        if (!states.TryGetValue(s, out var list))
                        {
                            list = new List<ViewLifecycleState>();
                            states[s] = list;
                        }

                        list.Add(s);
                        v.View.ShouldEqual(expectedView);
                        st.ShouldEqual(_viewModel);
                        m.ShouldEqual(Metadata);
                    }
                };
                ViewManager.AddComponent(listener);
            }

            for (var i = 0; i < viewCount; i++)
            {
                expectedView = results[0];
                await ViewManager.TryCleanupAsync(results[0], _viewModel, CancellationToken.None, Metadata);
                results.RemoveAt(0);
            }

            states.Count.ShouldEqual(2);
            states[ViewLifecycleState.Clearing].Count.ShouldEqual(count * viewCount);
            states[ViewLifecycleState.Cleared].Count.ShouldEqual(count * viewCount);
        }

        [Fact]
        public void TryInitializeAsyncShouldIgnoreNull() =>
            ViewManager.TryInitializeAsync(_view.Mapping, new ViewModelViewRequest(null, null), CancellationToken.None, Metadata).ShouldEqual(default);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task TryInitializeAsyncShouldInitializeViewAndNotifyListeners(int count)
        {
            var view = new object();
            var clearView = view;

            var states = new Dictionary<ViewLifecycleState, List<ViewLifecycleState>>();
            for (var i = 0; i < count; i++)
            {
                var listener = new TestViewLifecycleListener
                {
                    OnLifecycleChanged = (_, vRaw, s, st, m) =>
                    {
                        var v = vRaw.View!;
                        if (!states.TryGetValue(s, out var list))
                        {
                            list = new List<ViewLifecycleState>();
                            states[s] = list;
                        }

                        list.Add(s);
                        v.Mapping.ShouldEqual(_view.Mapping);
                        if (s == ViewLifecycleState.Clearing || s == ViewLifecycleState.Cleared)
                            v.Target.ShouldEqual(clearView);
                        else
                            v.Target.ShouldEqual(view);
                        m.ShouldEqual(Metadata);
                    },
                    Priority = i
                };
                ViewManager.AddComponent(listener);
            }

            var result = await ViewManager.TryInitializeAsync(_view.Mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, Metadata);
            result!.Mapping.ShouldEqual(_view.Mapping);
            result.Target.ShouldEqual(view);
            result.ViewModel.ShouldEqual(_viewModel);
            states.Count.ShouldEqual(2);
            states[ViewLifecycleState.Initializing].Count.ShouldEqual(count);
            states[ViewLifecycleState.Initialized].Count.ShouldEqual(count);

            states.Clear();
            result = await ViewManager.TryInitializeAsync(_view.Mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, Metadata);
            result!.Mapping.ShouldEqual(_view.Mapping);
            result.Target.ShouldEqual(view);
            result.ViewModel.ShouldEqual(_viewModel);
            states.Count.ShouldEqual(0);

            view = new object();
            result = await ViewManager.TryInitializeAsync(_view.Mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, Metadata);
            result!.Mapping.ShouldEqual(_view.Mapping);
            result.Target.ShouldEqual(view);
            result.ViewModel.ShouldEqual(_viewModel);
            states[ViewLifecycleState.Initializing].Count.ShouldEqual(count);
            states[ViewLifecycleState.Initialized].Count.ShouldEqual(count);
            states[ViewLifecycleState.Clearing].Count.ShouldEqual(count);
            states[ViewLifecycleState.Cleared].Count.ShouldEqual(count);
        }

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);
    }
}