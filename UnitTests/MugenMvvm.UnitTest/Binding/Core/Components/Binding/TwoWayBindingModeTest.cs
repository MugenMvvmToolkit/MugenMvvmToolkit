using System;
using System.Linq;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Observation.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components.Binding
{
    public class TwoWayBindingModeTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldUpdateBindingIfTargetAvailable()
        {
            var updateTargetCount = 0;
            var updateSourceCount = 0;
            var binding = new TestBinding
            {
                UpdateSource = () => ++updateSourceCount,
                UpdateTarget = () => ++updateTargetCount
            };

            IBindingSourceObserverListener sourceMode = TwoWayBindingMode.Instance;
            binding.AddComponent(sourceMode);
            updateTargetCount.ShouldEqual(1);
            updateSourceCount.ShouldEqual(0);
            binding.GetComponents<object>().Single().ShouldEqual(sourceMode);

            sourceMode.OnSourceLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateTargetCount.ShouldEqual(2);

            sourceMode.OnSourcePathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateTargetCount.ShouldEqual(3);

            sourceMode.OnSourceError(binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            updateTargetCount.ShouldEqual(3);


            IBindingTargetObserverListener mode = TwoWayBindingMode.Instance;
            mode.OnTargetLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateSourceCount.ShouldEqual(1);

            mode.OnTargetPathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateSourceCount.ShouldEqual(2);

            mode.OnTargetError(binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            updateSourceCount.ShouldEqual(2);
        }

        #endregion
    }
}