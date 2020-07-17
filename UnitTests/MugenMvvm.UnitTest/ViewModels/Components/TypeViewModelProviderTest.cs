using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.ViewModels.Components
{
    public class TypeViewModelProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldIgnoreNonGuidRequest()
        {
            var component = new TypeViewModelProvider();
            component.TryGetViewModel(null!, this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void ShouldUseServiceResolverAndNotifyLifecycle()
        {
            var viewModel = new TestViewModel();
            var lifeCycles = new List<ViewModelLifecycleState>();
            var manager = new ViewModelManager();
            manager.AddComponent(new TestViewModelLifecycleDispatcherComponent
            {
                OnLifecycleChanged = (vm, state, arg4, m) =>
                {
                    vm.ShouldEqual(viewModel);
                    m.ShouldEqual(DefaultMetadata);
                    lifeCycles.Add(state);
                }
            });
            manager.AddComponent(new TypeViewModelProvider(new TestServiceProvider
            {
                GetService = type =>
                {
                    type.ShouldEqual(viewModel.GetType());
                    return viewModel;
                }
            }));

            manager.TryGetViewModel(viewModel.GetType(), DefaultMetadata).ShouldEqual(viewModel);
            lifeCycles.Count.ShouldEqual(2);
            lifeCycles[0].ShouldEqual(ViewModelLifecycleState.Initializing);
            lifeCycles[1].ShouldEqual(ViewModelLifecycleState.Initialized);
        }

        #endregion
    }
}