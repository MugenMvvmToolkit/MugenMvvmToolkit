using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.UnitTest.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views.Components
{
    public class RawViewLifecycleDispatcherTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldResendEvent()
        {
            var state = "test";
            var st = ViewLifecycleState.Initialized;
            var viewModel = new TestViewModel();
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), this, viewModel);
            var invokeCount = 0;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new RawViewLifecycleDispatcher());
            viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (m, o, type, arg3) =>
                {
                    m.ShouldEqual(viewManager);
                    o.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    return view;
                }
            });
            viewManager.AddComponent(new TestViewLifecycleDispatcherComponent
            {
                OnLifecycleChanged = (m, o, lifecycleState, arg3, arg4, arg5) =>
                {
                    if (o == this)
                        return;
                    ++invokeCount;
                    m.ShouldEqual(viewManager);
                    o.ShouldEqual(view);
                    lifecycleState.ShouldEqual(st);
                    arg3.ShouldEqual(state);
                    arg5.ShouldEqual(DefaultMetadata);
                }
            });

            viewManager.OnLifecycleChanged(this, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            viewManager.OnLifecycleChanged(view, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}