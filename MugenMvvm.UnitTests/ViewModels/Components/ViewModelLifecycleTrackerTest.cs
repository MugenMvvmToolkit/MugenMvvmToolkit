using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class ViewModelLifecycleTrackerTest : UnitTestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackLifecycle(bool viewModelBase)
        {
            var manager = new ViewModelManager(ComponentCollectionManager);
            var viewModel = viewModelBase ? new TestViewModelBase(manager) : (IViewModelBase) new TestViewModel();
            var component = new ViewModelLifecycleTracker();
            manager.AddComponent(component);

            viewModel.IsInState(ViewModelLifecycleState.Created, null, manager).ShouldBeTrue();
            viewModel.IsInState(ViewModelLifecycleState.Disposed, null, manager).ShouldBeFalse();

            if (viewModelBase)
                ((IDisposable) viewModel).Dispose();
            else
                manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed);
            viewModel.IsInState(ViewModelLifecycleState.Disposed, null, manager).ShouldBeTrue();
        }
    }
}