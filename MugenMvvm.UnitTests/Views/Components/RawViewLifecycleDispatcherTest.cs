using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class RawViewLifecycleDispatcherTest : UnitTestBase
    {
        private readonly View _view;
        private readonly ViewManager _viewManager;

        public RawViewLifecycleDispatcherTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _view = new View(new ViewMapping("1", typeof(IViewModelBase), GetType()), this, new TestViewModel());
            _viewManager = new ViewManager(ComponentCollectionManager);
            _viewManager.AddComponent(new RawViewLifecycleDispatcher());
        }

        [Fact]
        public void ShouldResendEvent1()
        {
            var state = "test";
            var st = ViewLifecycleState.Initialized;
            var invokeCount = 0;

            _viewManager.AddComponent(new TestViewLifecycleListener
            {
                OnLifecycleChanged = (o, lifecycleState, arg3, arg5) =>
                {
                    if (o == this)
                        return;
                    ++invokeCount;
                    o.ShouldEqual(_view);
                    lifecycleState.ShouldEqual(st);
                    arg3.ShouldEqual(state);
                    arg5.ShouldEqual(DefaultMetadata);
                }
            });

            _viewManager.OnLifecycleChanged(this, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            _viewManager.OnLifecycleChanged(_view, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldResendEvent2()
        {
            var state = "test";
            var st = ViewLifecycleState.Initialized;
            var invokeCount = 0;

            _viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, arg3) =>
                {
                    o.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    return _view;
                }
            });
            _viewManager.AddComponent(new TestViewLifecycleListener
            {
                OnLifecycleChanged = (o, lifecycleState, arg3, arg5) =>
                {
                    if (o == this)
                        return;
                    ++invokeCount;
                    o.ShouldEqual(_view);
                    lifecycleState.ShouldEqual(st);
                    arg3.ShouldEqual(state);
                    arg5.ShouldEqual(DefaultMetadata);
                }
            });

            _viewManager.OnLifecycleChanged(this, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            _viewManager.OnLifecycleChanged(_view, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }
    }
}