using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
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
        private readonly ViewModelManager _viewModelManager;

        public ViewModelLifecycleTrackerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _viewModelManager.AddComponent(new ViewModelLifecycleTracker());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackInitializedLifecycle(bool viewModelBase)
        {
            var viewModel = viewModelBase ? new TestViewModelBase(_viewModelManager) : (IViewModelBase) new TestViewModel();

            viewModel.IsInState(ViewModelLifecycleState.Created, null, _viewModelManager).ShouldBeTrue();
            viewModel.IsInState(ViewModelLifecycleState.Initialized, null, _viewModelManager).ShouldBeFalse();

            _viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Initialized);
            viewModel.IsInState(ViewModelLifecycleState.Initialized, null, _viewModelManager).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackDisposeLifecycle(bool viewModelBase)
        {
            var viewModel = viewModelBase ? new TestViewModelBase(_viewModelManager) : (IViewModelBase) new TestViewModel();

            viewModel.IsInState(ViewModelLifecycleState.Created, null, _viewModelManager).ShouldBeTrue();
            viewModel.IsInState(ViewModelLifecycleState.Disposed, null, _viewModelManager).ShouldBeFalse();

            if (viewModelBase)
                ((IDisposable) viewModel).Dispose();
            else
                _viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed);
            viewModel.IsInState(ViewModelLifecycleState.Disposed, null, _viewModelManager).ShouldBeTrue();
        }
    }
}