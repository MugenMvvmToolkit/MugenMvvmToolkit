using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.ViewModels.Components
{
    public class ViewModelLifecycleTrackerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldTrackLifecycle()
        {
            var viewModel = new TestViewModel();
            var manager = new ViewModelManager();
            var component = new ViewModelLifecycleTracker();
            manager.AddComponent(component);

            manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Created, this);
            viewModel.Metadata.Get(ViewModelMetadata.LifecycleState).ShouldEqual(ViewModelLifecycleState.Created);

            manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Metadata.Get(ViewModelMetadata.LifecycleState).ShouldEqual(ViewModelLifecycleState.Disposed);
        }

        #endregion
    }
}