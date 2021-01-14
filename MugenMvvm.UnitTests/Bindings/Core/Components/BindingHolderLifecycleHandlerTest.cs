using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingHolderLifecycleHandlerTest : UnitTestBase
    {
        [Fact]
        public void OnLifecycleChangedShouldBeHandledByBindingHolderComponent()
        {
            var binding = new TestBinding();
            var target = new object();
            var manager = new BindingManager();

            var registerCount = 0;
            var unregisterCount = 0;
            var holder = new TestBindingHolderComponent(manager)
            {
                TryRegister = (o, b, arg3) =>
                {
                    ++registerCount;
                    o.ShouldEqual(target);
                    b.ShouldEqual(binding);
                    arg3.ShouldEqual(DefaultMetadata);
                    return true;
                },
                TryUnregister = (o, b, arg3) =>
                {
                    ++unregisterCount;
                    o.ShouldEqual(target);
                    b.ShouldEqual(binding);
                    arg3.ShouldEqual(DefaultMetadata);
                    return true;
                }
            };

            manager.AddComponent(holder);
            manager.AddComponent(new BindingHolderLifecycleHandler());

            binding.Target = new TestMemberPathObserver {Target = target};
            manager.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, this, DefaultMetadata);
            registerCount.ShouldEqual(1);
            unregisterCount.ShouldEqual(0);

            manager.OnLifecycleChanged(binding, BindingLifecycleState.Disposed, this, DefaultMetadata);
            registerCount.ShouldEqual(1);
            unregisterCount.ShouldEqual(1);

            manager.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, this, BindingMetadata.SuppressHolderRegistration.ToContext(true));
            registerCount.ShouldEqual(1);
            unregisterCount.ShouldEqual(1);

            manager.OnLifecycleChanged(binding, BindingLifecycleState.Disposed, this, BindingMetadata.SuppressHolderRegistration.ToContext(true));
            registerCount.ShouldEqual(1);
            unregisterCount.ShouldEqual(1);
        }
    }
}