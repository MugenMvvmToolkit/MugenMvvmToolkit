using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Core;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingHolderLifecycleHandlerTest : UnitTestBase
    {
        [Fact]
        public void OnLifecycleChangedShouldBeHandledByBindingHolderComponent()
        {
            var target = new object();
            var registerCount = 0;
            var unregisterCount = 0;

            BindingManager.AddComponent(new TestBindingHolderComponent
            {
                TryRegister = (m, o, b, arg3) =>
                {
                    ++registerCount;
                    m.ShouldEqual(BindingManager);
                    o.ShouldEqual(target);
                    b.ShouldEqual(Binding);
                    arg3.ShouldEqual(Metadata);
                    return true;
                },
                TryUnregister = (m, o, b, arg3) =>
                {
                    ++unregisterCount;
                    m.ShouldEqual(BindingManager);
                    o.ShouldEqual(target);
                    b.ShouldEqual(Binding);
                    arg3.ShouldEqual(Metadata);
                    return true;
                }
            });
            BindingManager.AddComponent(new BindingHolderLifecycleHandler());

            Binding.Target = new TestMemberPathObserver { Target = target };
            BindingManager.OnLifecycleChanged(Binding, BindingLifecycleState.Initialized, this, Metadata);
            registerCount.ShouldEqual(1);
            unregisterCount.ShouldEqual(0);

            BindingManager.OnLifecycleChanged(Binding, BindingLifecycleState.Disposed, this, Metadata);
            registerCount.ShouldEqual(1);
            unregisterCount.ShouldEqual(1);

            BindingManager.OnLifecycleChanged(Binding, BindingLifecycleState.Initialized, this, BindingMetadata.SuppressHolderRegistration.ToContext(true));
            registerCount.ShouldEqual(1);
            unregisterCount.ShouldEqual(1);

            BindingManager.OnLifecycleChanged(Binding, BindingLifecycleState.Disposed, this, BindingMetadata.SuppressHolderRegistration.ToContext(true));
            registerCount.ShouldEqual(1);
            unregisterCount.ShouldEqual(1);
        }
    }
}