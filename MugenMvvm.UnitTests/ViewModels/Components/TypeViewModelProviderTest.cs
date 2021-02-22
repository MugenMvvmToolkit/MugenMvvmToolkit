using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class TypeViewModelProviderTest : UnitTestBase
    {
        private readonly ViewModelManager _viewModelManager;
        private readonly TestServiceProvider _serviceProvider;

        public TypeViewModelProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _serviceProvider = new TestServiceProvider();
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _viewModelManager.AddComponent(new TypeViewModelProvider(_serviceProvider));
        }

        [Fact]
        public void ShouldIgnoreNonGuidRequest() => _viewModelManager.TryGetViewModel(this, DefaultMetadata).ShouldBeNull();

        [Fact]
        public void ShouldUseServiceResolverAndCheckInitializedState()
        {
            var viewModel = new TestViewModel();
            _viewModelManager.AddComponent(new TestViewModelLifecycleListener
            {
                OnLifecycleChanged = (_, _, _, _) => throw new NotSupportedException()
            });
            _viewModelManager.Components.Add(new TestLifecycleTrackerComponent<ViewModelLifecycleState>
            {
                IsInState = (_, vm, st, m) =>
                {
                    m.ShouldEqual(DefaultMetadata);
                    vm.ShouldEqual(viewModel);
                    return st == ViewModelLifecycleState.Initialized;
                }
            });
            _serviceProvider.GetService = type =>
            {
                type.ShouldEqual(viewModel.GetType());
                return viewModel;
            };

            _viewModelManager.TryGetViewModel(viewModel.GetType(), DefaultMetadata).ShouldEqual(viewModel);
        }

        [Fact]
        public void ShouldUseServiceResolverAndNotifyLifecycle()
        {
            var viewModel = new TestViewModel();
            var lifecycleStates = new List<ViewModelLifecycleState>();

            _viewModelManager.AddComponent(new TestViewModelLifecycleListener
            {
                OnLifecycleChanged = (vm, state, _, m) =>
                {
                    vm.ShouldEqual(viewModel);
                    m.ShouldEqual(DefaultMetadata);
                    lifecycleStates.Add(state);
                }
            });
            _serviceProvider.GetService = type =>
            {
                type.ShouldEqual(viewModel.GetType());
                return viewModel;
            };

            _viewModelManager.TryGetViewModel(viewModel.GetType(), DefaultMetadata).ShouldEqual(viewModel);
            lifecycleStates.Count.ShouldEqual(2);
            lifecycleStates[0].ShouldEqual(ViewModelLifecycleState.Initializing);
            lifecycleStates[1].ShouldEqual(ViewModelLifecycleState.Initialized);
        }
    }
}