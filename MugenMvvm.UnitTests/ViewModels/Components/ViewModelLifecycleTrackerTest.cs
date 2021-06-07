using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class ViewModelLifecycleTrackerTest : UnitTestBase
    {
        public ViewModelLifecycleTrackerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ViewModelManager.AddComponent(new ViewModelLifecycleTracker());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackInitializedLifecycle(bool viewModelBase)
        {
            var viewModel = viewModelBase ? new TestViewModelBase(ViewModelManager) : (IViewModelBase)new TestViewModel();

            viewModel.IsInState(ViewModelLifecycleState.Created, null, ViewModelManager).ShouldBeTrue();
            viewModel.IsInState(ViewModelLifecycleState.Initialized, null, ViewModelManager).ShouldBeFalse();

            ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Initialized);
            viewModel.IsInState(ViewModelLifecycleState.Initialized, null, ViewModelManager).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackDisposeLifecycle(bool viewModelBase)
        {
            var viewModel = viewModelBase ? new TestViewModelBase(ViewModelManager) : (IViewModelBase)new TestViewModel();

            viewModel.IsInState(ViewModelLifecycleState.Created, null, ViewModelManager).ShouldBeTrue();
            viewModel.IsInState(ViewModelLifecycleState.Disposed, null, ViewModelManager).ShouldBeFalse();

            if (viewModelBase)
                ((IDisposable)viewModel).Dispose();
            else
                ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed);
            viewModel.IsInState(ViewModelLifecycleState.Disposed, null, ViewModelManager).ShouldBeTrue();
        }

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);
    }
}