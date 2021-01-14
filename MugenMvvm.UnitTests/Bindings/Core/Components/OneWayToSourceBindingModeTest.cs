using System;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class OneWayToSourceBindingModeTest : UnitTestBase
    {
        [Fact]
        public void ShouldUpdateBindingIfTargetAvailable()
        {
            var updateCount = 0;
            var binding = new TestBinding
            {
                UpdateTarget = () => throw new NotSupportedException(),
                UpdateSource = () => ++updateCount,
                Source = ItemOrArray.FromItem<object?>(new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                })
            };

            IBindingTargetObserverListener mode = OneWayToSourceBindingMode.Instance;
            binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            binding.GetComponents<object>().Single().ShouldEqual(mode);

            mode.OnTargetLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(2);

            mode.OnTargetPathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(3);

            mode.OnTargetError(binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            updateCount.ShouldEqual(3);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldUpdateBindingIfNotAvailable(bool lastMember)
        {
            var updateCount = 0;
            var isAvailable = false;
            var binding = new TestBinding
            {
                UpdateTarget = () => throw new NotSupportedException(),
                UpdateSource = () => ++updateCount,
                Source = ItemOrArray.FromItem<object?>(new TestMemberPathObserver
                {
                    GetLastMember = metadata =>
                    {
                        if (isAvailable)
                            return new MemberPathLastMember(this, ConstantMemberInfo.Target);
                        return default;
                    }
                })
            };

            IBindingTargetObserverListener mode = OneWayToSourceBindingMode.Instance;
            var oneTimeMode = OneWayToSourceBindingMode.OneTimeHandlerComponent.Instance;
            binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnSourceLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                oneTimeMode.OnSourcePathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);

            updateCount.ShouldEqual(1);
            binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            isAvailable = true;
            oneTimeMode.OnSourceError(binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            updateCount.ShouldEqual(1);
            binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnSourceLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                oneTimeMode.OnSourcePathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(2);
        }
    }
}