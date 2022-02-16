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
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldUpdateBindingIfTargetAvailable(bool source)
        {
            var mode = source ? TwoWayBindingMode.Source : TwoWayBindingMode.Target;
            var updateTargetCount = 0;
            var updateSourceCount = 0;
            Binding.UpdateSource = () => ++updateSourceCount;
            Binding.UpdateTarget = () => ++updateTargetCount;

            IBindingSourceObserverListener sourceMode = mode;
            Binding.AddComponent(sourceMode);
            updateTargetCount.ShouldEqual(source ? 0 : 1);
            updateSourceCount.ShouldEqual(source ? 1 : 0);
            Binding.GetComponents<object>().Single().ShouldEqual(sourceMode);

            updateTargetCount = 0;
            updateSourceCount = 0;
            sourceMode.OnSourceLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateTargetCount.ShouldEqual(1);

            sourceMode.OnSourcePathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateTargetCount.ShouldEqual(2);

            sourceMode.OnSourceError(Binding, EmptyPathObserver.Empty, new Exception(), Metadata);
            updateTargetCount.ShouldEqual(2);


            IBindingTargetObserverListener targetMode = mode;
            targetMode.OnTargetLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateSourceCount.ShouldEqual(1);

            targetMode.OnTargetPathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateSourceCount.ShouldEqual(2);

            targetMode.OnTargetError(Binding, EmptyPathObserver.Empty, new Exception(), Metadata);
            updateSourceCount.ShouldEqual(2);
        }
    }
}