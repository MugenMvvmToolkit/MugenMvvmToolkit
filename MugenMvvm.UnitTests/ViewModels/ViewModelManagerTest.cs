using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.ViewModels
{
    public class ViewModelManagerTest : ComponentOwnerTestBase<ViewModelManager>
    {
        [Fact]
        public void GetServiceShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => GetComponentOwner(ComponentCollectionManager).GetService(new TestViewModel(), typeof(object), DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsInStateShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var owner = GetComponentOwner(ComponentCollectionManager);
            var target = new TestViewModel();
            var state = ViewModelLifecycleState.Created;

            owner.IsInState(target, state, DefaultMetadata).ShouldBeFalse();

            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i - 1 == componentCount;
                var component = new TestLifecycleTrackerComponent<ViewModelLifecycleState>(owner)
                {
                    IsInState = (o, t, s, m) =>
                    {
                        ++count;
                        t.ShouldEqual(target);
                        m.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                };
                owner.Components.TryAdd(component);
            }

            owner.IsInState(target, state, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var manager = GetComponentOwner(ComponentCollectionManager);
            var invokeCount = 0;
            var state = "state";
            var viewModel = new TestViewModel();
            var lifecycleState = ViewModelLifecycleState.Created;
            for (var i = 0; i < count; i++)
            {
                var component = new TestViewModelLifecycleListener(manager)
                {
                    OnLifecycleChanged = (vm, viewModelLifecycleState, st, metadata) =>
                    {
                        ++invokeCount;
                        vm.ShouldEqual(viewModel);
                        st.ShouldEqual(state);
                        viewModelLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                };
                manager.AddComponent(component);
            }

            manager.OnLifecycleChanged(viewModel, lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetServiceShouldBeHandledByComponents(int count)
        {
            var manager = GetComponentOwner(ComponentCollectionManager);
            var viewModel = new TestViewModel();
            var service = new object();
            var executeCount = 0;
            var serviceType = typeof(bool);
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestViewModelServiceProviderComponent(manager)
                {
                    TryGetService = (vm, r, ctx) =>
                    {
                        ++executeCount;
                        vm.ShouldEqual(viewModel);
                        r.ShouldEqual(serviceType);
                        ctx.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return service;
                        return null;
                    },
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            manager.GetService(viewModel, serviceType, DefaultMetadata).ShouldEqual(service);
            executeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetViewModelShouldBeHandledByComponents(int count)
        {
            var manager = GetComponentOwner(ComponentCollectionManager);
            var viewModel = new TestViewModel();
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestViewModelProviderComponent
                {
                    TryGetViewModel = (o, arg3) =>
                    {
                        ++executeCount;
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return viewModel;
                        return null;
                    },
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            manager.TryGetViewModel(this, DefaultMetadata).ShouldEqual(viewModel);
            executeCount.ShouldEqual(count);
        }

        protected override ViewModelManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}