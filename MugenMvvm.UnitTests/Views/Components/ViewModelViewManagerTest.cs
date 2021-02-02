using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
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
        private readonly ViewManager _viewManager;

        public ViewModelViewManagerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata);
            _view = new View(mapping, new object(), _viewModel);
            _viewManager = new ViewManager(ComponentCollectionManager);
            _viewManager.AddComponent(new ViewModelViewManager(AttachedValueManager, ComponentCollectionManager));
        }

        [Fact]
        public void TryInitializeAsyncShouldIgnoreNull() =>
            _viewManager.TryInitializeAsync(_view.Mapping, new ViewModelViewRequest(null, null), CancellationToken.None, DefaultMetadata).ShouldEqual(default);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ShouldInitializeCleanupGetViews(int count)
        {
            var results = new List<IView>();
            for (var i = 0; i < count; i++)
            {
                var view = new object();
                var mapping = new ViewMapping("id" + i, typeof(TestViewModel), typeof(object), DefaultMetadata);
                var result = (await _viewManager.TryInitializeAsync(mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, DefaultMetadata))!;
                results.Add(result);

                result.Mapping.ShouldEqual(mapping);
                result.ViewModel.ShouldEqual(_viewModel);

                _viewManager.GetViews(_viewModel, DefaultMetadata).AsList().ShouldContain(results);
                _viewManager.GetViews(view, DefaultMetadata).AsList().Single().ShouldEqual(results.Last());
            }

            for (var i = 0; i < count; i++)
            {
                var view = results[0];
                _viewManager.GetViews(_viewModel, DefaultMetadata).AsList().ShouldContain(results);
                _viewManager.GetViews(view.Target, DefaultMetadata).AsList().Single().ShouldEqual(view);
                await _viewManager.TryCleanupAsync(view, _viewModel, CancellationToken.None, DefaultMetadata);
                _viewManager.GetViews(view.Target, DefaultMetadata).AsList().ShouldBeEmpty();
                results.RemoveAt(0);
            }

            _viewManager.GetViews(_viewModel, DefaultMetadata).AsList().ShouldBeEmpty();
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
                var mapping = new ViewMapping("id" + i, typeof(TestViewModel), typeof(object), DefaultMetadata);
                results.Add((await _viewManager.TryInitializeAsync(mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, DefaultMetadata))!);
            }

            var states = new Dictionary<ViewLifecycleState, List<ViewLifecycleState>>();
            IView? expectedView = null;
            for (var i = 0; i < count; i++)
            {
                var listener = new TestViewLifecycleListener
                {
                    OnLifecycleChanged = (v, s, st, m) =>
                    {
                        if (!states.TryGetValue(s, out var list))
                        {
                            list = new List<ViewLifecycleState>();
                            states[s] = list;
                        }

                        list.Add(s);
                        v.ShouldEqual(expectedView);
                        st.ShouldEqual(_viewModel);
                        m.ShouldEqual(DefaultMetadata);
                    }
                };
                _viewManager.AddComponent(listener);
            }

            for (var i = 0; i < viewCount; i++)
            {
                expectedView = results[0];
                await _viewManager.TryCleanupAsync(results[0], _viewModel, CancellationToken.None, DefaultMetadata);
                results.RemoveAt(0);
            }

            states.Count.ShouldEqual(2);
            states[ViewLifecycleState.Clearing].Count.ShouldEqual(count * viewCount);
            states[ViewLifecycleState.Cleared].Count.ShouldEqual(count * viewCount);
        }

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
                    OnLifecycleChanged = (vRaw, s, st, m) =>
                    {
                        var v = (IView) vRaw;
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
                        m.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                };
                _viewManager.AddComponent(listener);
            }

            var result = await _viewManager.TryInitializeAsync(_view.Mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, DefaultMetadata);
            result!.Mapping.ShouldEqual(_view.Mapping);
            result.Target.ShouldEqual(view);
            result.ViewModel.ShouldEqual(_viewModel);
            states.Count.ShouldEqual(2);
            states[ViewLifecycleState.Initializing].Count.ShouldEqual(count);
            states[ViewLifecycleState.Initialized].Count.ShouldEqual(count);

            states.Clear();
            result = await _viewManager.TryInitializeAsync(_view.Mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, DefaultMetadata);
            result!.Mapping.ShouldEqual(_view.Mapping);
            result.Target.ShouldEqual(view);
            result.ViewModel.ShouldEqual(_viewModel);
            states.Count.ShouldEqual(0);

            view = new object();
            result = await _viewManager.TryInitializeAsync(_view.Mapping, new ViewModelViewRequest(_viewModel, view), CancellationToken.None, DefaultMetadata);
            result!.Mapping.ShouldEqual(_view.Mapping);
            result.Target.ShouldEqual(view);
            result.ViewModel.ShouldEqual(_viewModel);
            states[ViewLifecycleState.Initializing].Count.ShouldEqual(count);
            states[ViewLifecycleState.Initialized].Count.ShouldEqual(count);
            states[ViewLifecycleState.Clearing].Count.ShouldEqual(count);
            states[ViewLifecycleState.Cleared].Count.ShouldEqual(count);
        }
    }
}