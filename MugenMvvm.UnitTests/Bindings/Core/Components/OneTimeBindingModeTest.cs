using System;
using System.Linq;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class OneTimeBindingModeTest : UnitTestBase
    {
        public OneTimeBindingModeTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnAttachingShouldUpdateBindingIfAvailable(bool dispose)
        {
            var disposeCount = 0;
            var updateCount = 0;

            Binding.UpdateSource = () => throw new NotSupportedException();
            Binding.UpdateTarget = () => ++updateCount;
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
            };
            Binding.Source = new[]
            {
                new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                },
                new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                }
            };
            Binding.Dispose = () => ++disposeCount;

            var mode = dispose ? OneTimeBindingMode.Instance : OneTimeBindingMode.NonDisposeInstance;
            ((IHasAttachConditionComponent) mode).CanAttach(Binding, Metadata).ShouldBeFalse();
            disposeCount.ShouldEqual(dispose ? 1 : 0);
            updateCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void OnAttachingShouldUpdateBindingIfNotAvailable(bool dispose, bool lastMemberChanged)
        {
            var disposeCount = 0;
            var updateCount = 0;
            var isAvailableTarget = false;
            var isAvailableSource = false;

            Binding.UpdateSource = () => throw new NotSupportedException();
            Binding.UpdateTarget = () => ++updateCount;
            Binding.Target = new TestMemberPathObserver
            {
                GetLastMember = metadata =>
                {
                    if (isAvailableTarget)
                        return new MemberPathLastMember(this, ConstantMemberInfo.Target);
                    return default;
                }
            };
            Binding.Source = new[]
            {
                new TestMemberPathObserver
                {
                    GetLastMember = metadata =>
                    {
                        if (isAvailableSource)
                            return new MemberPathLastMember(this, ConstantMemberInfo.Target);
                        return default;
                    }
                },
                new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                }
            };
            Binding.Dispose = () => ++disposeCount;
            var mode = dispose ? OneTimeBindingMode.Instance : OneTimeBindingMode.NonDisposeInstance;
            var listener = (IBindingSourceObserverListener) mode;
            Binding.AddComponent(mode);
            Binding.GetComponents<object>().Single().ShouldEqual(mode);

            isAvailableSource = true;
            if (lastMemberChanged)
                listener.OnSourceLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            else
                listener.OnSourcePathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            disposeCount.ShouldEqual(0);
            updateCount.ShouldEqual(0);

            isAvailableTarget = true;
            listener.OnSourceError(Binding, EmptyPathObserver.Empty, new Exception(), Metadata);
            disposeCount.ShouldEqual(0);
            updateCount.ShouldEqual(0);

            if (lastMemberChanged)
                listener.OnSourceLastMemberChanged(Binding, EmptyPathObserver.Empty, Metadata);
            else
                listener.OnSourcePathMembersChanged(Binding, EmptyPathObserver.Empty, Metadata);
            disposeCount.ShouldEqual(dispose ? 1 : 0);
            updateCount.ShouldEqual(1);
        }
    }
}