using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views.Components
{
    public class ViewLifecycleTrackerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldTrackLifecycle()
        {
            var viewModel = new TestViewModel();
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), this, viewModel);
            var manager = new ViewManager();
            var component = new ViewLifecycleTracker();
            manager.AddComponent(component);

            manager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this);
            view.Metadata.Get(ViewMetadata.LifecycleState).ShouldEqual(ViewLifecycleState.Initialized);

            manager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, this);
            view.Metadata.Get(ViewMetadata.LifecycleState).ShouldEqual(ViewLifecycleState.Initializing);
        }

        #endregion
    }
}