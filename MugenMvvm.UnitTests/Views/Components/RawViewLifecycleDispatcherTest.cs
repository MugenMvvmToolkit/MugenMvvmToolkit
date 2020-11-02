using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class RawViewLifecycleDispatcherTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldResendEvent1()
        {
            var state = "test";
            var st = ViewLifecycleState.Initialized;
            var viewModel = new TestViewModel();
            var view = new View(new ViewMapping("1", GetType(), typeof(IViewModelBase)), this, viewModel);
            var invokeCount = 0;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new RawViewLifecycleDispatcher());
            viewManager.AddComponent(new TestViewLifecycleDispatcherComponent
            {
                OnLifecycleChanged = (o, lifecycleState, arg3, arg5) =>
                {
                    if (o == this)
                        return;
                    ++invokeCount;
                    o.ShouldEqual(view);
                    lifecycleState.ShouldEqual(st);
                    arg3.ShouldEqual(state);
                    arg5.ShouldEqual(DefaultMetadata);
                }
            });

            viewManager.OnLifecycleChanged(this, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, st, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldResendEvent2()
        {
            var state = "test";
            var st = ViewLifecycleState.Initialized;
            var viewModel = new TestViewModel();
            var view = new View(new ViewMapping("1", GetType(), typeof(IViewModelBase)), this, viewModel);
            var invokeCount = 0;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new RawViewLifecycleDispatcher());
            viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, arg3) =>
                {
                    o.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    return ItemOrList.FromItem<IView>(view);
                }
            });
            viewManager.AddComponent(new TestViewLifecycleDispatcherComponent
            {
                OnLifecycleChanged = (o, lifecycleState, arg3, arg5) =>
                {
                    if (o == this)
                        return;
                    ++invokeCount;
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