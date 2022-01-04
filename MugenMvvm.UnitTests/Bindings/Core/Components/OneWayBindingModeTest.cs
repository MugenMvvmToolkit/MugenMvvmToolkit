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

        [Fact]
        public void ShouldUpdateBindingIfNotAvailable()
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
            IBindingTargetObserverListener targetMember = OneWayBindingMode.Instance;
            Binding.AddComponent(mode);
            updateCount.ShouldEqual(1);

            targetMember.OnTargetPathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateCount.ShouldEqual(2);
            Binding.GetComponents<object>().ShouldContain(mode, targetMember);

            isAvailable = true;
            targetMember.OnTargetError(Binding, EmptyPathObserver.Empty, new Exception(), Metadata);
            updateCount.ShouldEqual(2);
            Binding.GetComponents<object>().ShouldContain(mode, targetMember);

            targetMember.OnTargetPathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateCount.ShouldEqual(3);
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

            mode.OnSourceLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateCount.ShouldEqual(2);

            mode.OnSourcePathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateCount.ShouldEqual(3);

            mode.OnSourceError(Binding, EmptyPathObserver.Empty, new Exception(), Metadata);
            updateCount.ShouldEqual(3);
        }
    }
}