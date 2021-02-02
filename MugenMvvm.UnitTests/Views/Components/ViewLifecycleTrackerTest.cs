using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewLifecycleTrackerTest : UnitTestBase
    {
        [Fact]
        public void ShouldTrackLifecycle()
        {
            var viewModel = new TestViewModel();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), GetType()), this, viewModel);
            var manager = new ViewManager(ComponentCollectionManager);
            manager.AddComponent(new ViewLifecycleTracker(AttachedValueManager));

            manager.IsInState(view, ViewLifecycleState.Appeared).ShouldBeFalse();
            manager.IsInState(view, ViewLifecycleState.Disappeared).ShouldBeFalse();
            manager.IsInState(view, ViewLifecycleState.Closed).ShouldBeFalse();

            manager.OnLifecycleChanged(view, ViewLifecycleState.Appeared, this);
            manager.IsInState(view, ViewLifecycleState.Appeared).ShouldBeTrue();
            manager.IsInState(view, ViewLifecycleState.Disappeared).ShouldBeFalse();
            manager.IsInState(view, ViewLifecycleState.Closed).ShouldBeFalse();

            manager.OnLifecycleChanged(view, ViewLifecycleState.Disappeared, this);
            manager.IsInState(view, ViewLifecycleState.Appeared).ShouldBeFalse();
            manager.IsInState(view, ViewLifecycleState.Disappeared).ShouldBeTrue();
            manager.IsInState(view, ViewLifecycleState.Closed).ShouldBeFalse();

            manager.OnLifecycleChanged(view, ViewLifecycleState.Closed, this);
            manager.IsInState(view, ViewLifecycleState.Appeared).ShouldBeFalse();
            manager.IsInState(view, ViewLifecycleState.Disappeared).ShouldBeTrue();
            manager.IsInState(view, ViewLifecycleState.Closed).ShouldBeTrue();

            manager.OnLifecycleChanged(view, ViewLifecycleState.Appeared, this);
            manager.IsInState(view, ViewLifecycleState.Appeared).ShouldBeTrue();
            manager.IsInState(view, ViewLifecycleState.Disappeared).ShouldBeFalse();
            manager.IsInState(view, ViewLifecycleState.Closed).ShouldBeFalse();

            manager.OnLifecycleChanged(view, ViewLifecycleState.Closed, this);
            manager.IsInState(view, ViewLifecycleState.Appeared).ShouldBeFalse();
            manager.IsInState(view, ViewLifecycleState.Disappeared).ShouldBeFalse();
            manager.IsInState(view, ViewLifecycleState.Closed).ShouldBeTrue();
        }
    }
}