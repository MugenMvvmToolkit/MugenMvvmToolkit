using System;
using System.Linq;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class OneWayToSourceBindingModeTest : UnitTestBase
    {
        public OneWayToSourceBindingModeTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldUpdateBindingIfNotAvailable(bool lastMember)
        {
            var updateCount = 0;
            var isAvailable = false;

            Binding.UpdateTarget = () => throw new NotSupportedException();
            Binding.UpdateSource = () => ++updateCount;
            Binding.Source = ItemOrArray.FromItem<object?>(new TestMemberPathObserver
            {
                GetLastMember = metadata =>
                {
                    if (isAvailable)
                        return new MemberPathLastMember(this, ConstantMemberInfo.Target);
                    return default;
                }
            });

            IBindingTargetObserverListener mode = OneWayToSourceBindingMode.Instance;
            var oneTimeMode = OneWayToSourceBindingMode.OneTimeHandlerComponent.Instance;
            Binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            Binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnSourceLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            else
                oneTimeMode.OnSourcePathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);

            updateCount.ShouldEqual(1);
            Binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            isAvailable = true;
            oneTimeMode.OnSourceError(Binding, EmptyPathObserver.Empty, new Exception(), Metadata);
            updateCount.ShouldEqual(1);
            Binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnSourceLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            else
                oneTimeMode.OnSourcePathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateCount.ShouldEqual(2);
        }

        [Fact]
        public void ShouldUpdateBindingIfTargetAvailable()
        {
            var updateCount = 0;

            Binding.UpdateTarget = () => throw new NotSupportedException();
            Binding.UpdateSource = () => ++updateCount;
            Binding.Source = ItemOrArray.FromItem<object?>(new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
            });

            IBindingTargetObserverListener mode = OneWayToSourceBindingMode.Instance;
            Binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            Binding.GetComponents<object>().Single().ShouldEqual(mode);

            mode.OnTargetLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateCount.ShouldEqual(2);

            mode.OnTargetPathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            updateCount.ShouldEqual(3);

            mode.OnTargetError(Binding, EmptyPathObserver.Empty, new Exception(), Metadata);
            updateCount.ShouldEqual(3);
        }
    }
}