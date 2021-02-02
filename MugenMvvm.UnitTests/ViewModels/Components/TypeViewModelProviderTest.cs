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

        public TypeViewModelProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
        }

        [Fact]
        public void ShouldIgnoreNonGuidRequest()
        {
            _viewModelManager.AddComponent(new TypeViewModelProvider());
            _viewModelManager.TryGetViewModel(this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void ShouldUseServiceResolverAndNotifyLifecycle()
        {
            var viewModel = new TestViewModel();
            var lifeCycles = new List<ViewModelLifecycleState>();

            _viewModelManager.AddComponent(new TestViewModelLifecycleListener
            {
                OnLifecycleChanged = (vm, state, arg4, m) =>
                {
                    vm.ShouldEqual(viewModel);
                    m.ShouldEqual(DefaultMetadata);
                    lifeCycles.Add(state);
                }
            });
            _viewModelManager.AddComponent(new TypeViewModelProvider(new TestServiceProvider
            {
                GetService = type =>
                {
                    type.ShouldEqual(viewModel.GetType());
                    return viewModel;
                }
            }));

            _viewModelManager.TryGetViewModel(viewModel.GetType(), DefaultMetadata).ShouldEqual(viewModel);
            lifeCycles.Count.ShouldEqual(2);
            lifeCycles[0].ShouldEqual(ViewModelLifecycleState.Initializing);
            lifeCycles[1].ShouldEqual(ViewModelLifecycleState.Initialized);
        }
    }
}