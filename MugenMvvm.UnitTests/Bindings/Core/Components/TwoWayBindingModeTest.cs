using System;
using System.Linq;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class TwoWayBindingModeTest : UnitTestBase
    {
        [Fact]
        public void ShouldUpdateBindingIfTargetAvailable()
        {
            var updateTargetCount = 0;
            var updateSourceCount = 0;
            Binding.UpdateSource = () => ++updateSourceCount;
            Binding.UpdateTarget = () => ++updateTargetCount;

            IBindingSourceObserverListener sourceMode = TwoWayBindingMode.Instance;
            Binding.AddComponent(sourceMode);
            updateTargetCount.ShouldEqual(1);
            updateSourceCount.ShouldEqual(0);
            Binding.GetComponents<object>().Single().ShouldEqual(sourceMode);

            sourceMode.OnSourceLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateTargetCount.ShouldEqual(2);

            sourceMode.OnSourcePathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateTargetCount.ShouldEqual(3);

            sourceMode.OnSourceError(Binding, EmptyPathObserver.Empty, new Exception(), Metadata);
            updateTargetCount.ShouldEqual(3);


            IBindingTargetObserverListener mode = TwoWayBindingMode.Instance;
            mode.OnTargetLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateSourceCount.ShouldEqual(1);

            mode.OnTargetPathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateSourceCount.ShouldEqual(2);

            mode.OnTargetError(Binding, EmptyPathObserver.Empty, new Exception(), Metadata);
            updateSourceCount.ShouldEqual(2);
        }
    }
}