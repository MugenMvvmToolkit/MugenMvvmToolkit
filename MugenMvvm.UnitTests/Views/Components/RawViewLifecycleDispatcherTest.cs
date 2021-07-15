using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
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

        public RawViewLifecycleDispatcherTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _view = new View(new ViewMapping("1", typeof(IViewModelBase), GetType()), this, new TestViewModel());
            ViewManager.AddComponent(new RawViewLifecycleDispatcher());
        }

        [Fact]
        public void ShouldResendEvent1()
        {
            var state = "test";
            var st = ViewLifecycleState.Initialized;
            var invokeCount = 0;

            ViewManager.AddComponent(new TestViewLifecycleListener
            {
                OnLifecycleChanged = (m, o, lifecycleState, arg3, arg5) =>
                {
                    if (o.RawView == this)
                        return;
                    ++invokeCount;
                    m.ShouldEqual(ViewManager);
                    o.RawView.ShouldEqual(_view);
                    lifecycleState.ShouldEqual(st);
                    arg3.ShouldEqual(state);
                    arg5.ShouldEqual(DefaultMetadata);
                }
            });

            ViewManager.OnLifecycleChanged(this, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            ViewManager.OnLifecycleChanged(_view, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldResendEvent2()
        {
            var state = "test";
            var st = ViewLifecycleState.Initialized;
            var invokeCount = 0;

            ViewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (m, o, arg3) =>
                {
                    m.ShouldEqual(ViewManager);
                    o.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    return _view;
                }
            });
            ViewManager.AddComponent(new TestViewLifecycleListener
            {
                OnLifecycleChanged = (m, o, lifecycleState, arg3, arg5) =>
                {
                    if (o.RawView == this)
                        return;
                    ++invokeCount;
                    m.ShouldEqual(ViewManager);
                    o.RawView.ShouldEqual(_view);
                    lifecycleState.ShouldEqual(st);
                    arg3.ShouldEqual(state);
                    arg5.ShouldEqual(DefaultMetadata);
                }
            });

            ViewManager.OnLifecycleChanged(this, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            ViewManager.OnLifecycleChanged(_view, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);
    }
}