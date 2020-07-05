using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class BindingHolderLifecycleDispatcherTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void OnLifecycleChangedShouldBeHandledByBindingHolderComponent()
        {
            var binding = new TestBinding();
            var target = new object();
            var manager = new BindingManager();

            var registerCount = 0;
            var unregisterCount = 0;
            var holder = new TestBindingHolderComponent
            {
                TryRegister = (m, o, b, arg3) =>
                {
                    ++registerCount;
                    m.ShouldEqual(manager);
                    o.ShouldEqual(target);
                    b.ShouldEqual(binding);
                    arg3.ShouldEqual(DefaultMetadata);
                    return true;
                },
                TryUnregister = (m, o, b, arg3) =>
                {
                    ++unregisterCount;
                    m.ShouldEqual(manager);
                    o.ShouldEqual(target);
                    b.ShouldEqual(binding);
                    arg3.ShouldEqual(DefaultMetadata);
                    return true;
                }
            };

            manager.AddComponent(holder);
            manager.AddComponent(new BindingHolderLifecycleDispatcher());

            manager.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, new BindingTargetSourceState(target, null), DefaultMetadata);
            registerCount.ShouldEqual(1);
            unregisterCount.ShouldEqual(0);

            binding.Target = new TestMemberPathObserver { Target = target };
            manager.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, this, DefaultMetadata);
            registerCount.ShouldEqual(2);
            unregisterCount.ShouldEqual(0);

            manager.OnLifecycleChanged(binding, BindingLifecycleState.Disposed, this, DefaultMetadata);
            registerCount.ShouldEqual(2);
            unregisterCount.ShouldEqual(1);

            manager.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, this, MetadataContextValue.Create(BindingMetadata.SuppressHolderRegistration, true).ToContext());
            registerCount.ShouldEqual(2);
            unregisterCount.ShouldEqual(1);

            manager.OnLifecycleChanged(binding, BindingLifecycleState.Disposed, this, MetadataContextValue.Create(BindingMetadata.SuppressHolderRegistration, true).ToContext());
            registerCount.ShouldEqual(2);
            unregisterCount.ShouldEqual(1);
        }

        #endregion
    }
}