using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.ViewModels;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.ViewModels
{
    public class ViewModelManagerTest : ComponentOwnerTestBase<ViewModelManager>
    {
        [Fact]
        public void GetServiceShouldThrowNoComponents() =>
            ShouldThrow<InvalidOperationException>(() => ViewModelManager.GetService(new TestViewModel(), typeof(object), DefaultMetadata));

        protected override IViewModelManager GetViewModelManager() => GetComponentOwner(ComponentCollectionManager);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsInStateShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var target = new TestViewModel();
            var state = ViewModelLifecycleState.Created;

            ViewModelManager.IsInState(target, state, DefaultMetadata).ShouldBeFalse();

            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i - 1 == componentCount;
                ViewModelManager.Components.TryAdd(new TestLifecycleTrackerComponent<ViewModelLifecycleState>
                {
                    IsInState = (o, t, s, m) =>
                    {
                        ++count;
                        o.ShouldEqual(ViewModelManager);
                        t.ShouldEqual(target);
                        m.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                });
            }

            ViewModelManager.IsInState(target, state, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var state = "state";
            var viewModel = new TestViewModel();
            var lifecycleState = ViewModelLifecycleState.Created;
            for (var i = 0; i < count; i++)
            {
                ViewModelManager.AddComponent(new TestViewModelLifecycleListener
                {
                    OnLifecycleChanged = (m, vm, viewModelLifecycleState, st, metadata) =>
                    {
                        ++invokeCount;
                        m.ShouldEqual(ViewModelManager);
                        vm.ShouldEqual(viewModel);
                        st.ShouldEqual(state);
                        viewModelLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                });
            }

            ViewModelManager.OnLifecycleChanged(viewModel, lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetServiceShouldBeHandledByComponents(int count)
        {
            var viewModel = new TestViewModel();
            var service = new object();
            var executeCount = 0;
            var serviceType = typeof(bool);
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                ViewModelManager.AddComponent(new TestViewModelServiceProviderComponent
                {
                    TryGetService = (m, vm, r, ctx) =>
                    {
                        ++executeCount;
                        m.ShouldEqual(ViewModelManager);
                        vm.ShouldEqual(viewModel);
                        r.ShouldEqual(serviceType);
                        ctx.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return service;
                        return null;
                    },
                    Priority = -i
                });
            }

            ViewModelManager.GetService(viewModel, serviceType, DefaultMetadata).ShouldEqual(service);
            executeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetViewModelShouldBeHandledByComponents(int count)
        {
            var viewModel = new TestViewModel();
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                ViewModelManager.AddComponent(new TestViewModelProviderComponent
                {
                    TryGetViewModel = (m, o, arg3) =>
                    {
                        ++executeCount;
                        m.ShouldEqual(ViewModelManager);
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return viewModel;
                        return null;
                    },
                    Priority = -i
                });
            }

            ViewModelManager.TryGetViewModel(this, DefaultMetadata).ShouldEqual(viewModel);
            executeCount.ShouldEqual(count);
        }

        protected override ViewModelManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}