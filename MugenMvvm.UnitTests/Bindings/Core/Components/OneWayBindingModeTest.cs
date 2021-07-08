using System;
using System.Linq;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class OneWayBindingModeTest : UnitTestBase
    {
        public OneWayBindingModeTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldUpdateBindingIfNotAvailable(bool lastMember)
        {
            var updateCount = 0;
            var isAvailable = false;

            Binding.UpdateSource = () => throw new NotSupportedException();
            Binding.UpdateTarget = () => ++updateCount;
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata =>
                {
                    if (isAvailable)
                        return new MemberPathLastMember(this, ConstantMemberInfo.Target);
                    return default;
                }
            };

            IBindingSourceObserverListener mode = OneWayBindingMode.Instance;
            var oneTimeMode = OneWayBindingMode.OneTimeHandlerComponent.Instance;
            Binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            Binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnTargetLastMemberChanged(Binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                oneTimeMode.OnTargetPathMembersChanged(Binding, EmptyPathObserver.Empty, DefaultMetadata);

            updateCount.ShouldEqual(1);
            Binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            isAvailable = true;
            oneTimeMode.OnTargetError(Binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            updateCount.ShouldEqual(1);
            Binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnTargetLastMemberChanged(Binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                oneTimeMode.OnTargetPathMembersChanged(Binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(2);
        }

        [Fact]
        public void ShouldUpdateBindingIfTargetAvailable()
        {
            var updateCount = 0;

            Binding.UpdateSource = () => throw new NotSupportedException();
            Binding.UpdateTarget = () => ++updateCount;
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
            };

            IBindingSourceObserverListener mode = OneWayBindingMode.Instance;
            Binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            Binding.GetComponents<object>().Single().ShouldEqual(mode);

            mode.OnSourceLastMemberChanged(Binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(2);

            mode.OnSourcePathMembersChanged(Binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(3);

            mode.OnSourceError(Binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            updateCount.ShouldEqual(3);
        }
    }
}