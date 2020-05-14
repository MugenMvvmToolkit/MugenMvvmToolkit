using System;
using System.Linq;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components.Binding
{
    public class OneWayBindingModeTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldUpdateBindingIfTargetAvailable()
        {
            var updateCount = 0;
            var binding = new TestBinding
            {
                UpdateSource = () => throw new NotSupportedException(),
                UpdateTarget = () => ++updateCount,
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                }
            };

            IBindingSourceObserverListener mode = OneWayBindingMode.Instance;
            binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            binding.GetComponents<object>().Single().ShouldEqual(mode);

            mode.OnSourceLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(2);

            mode.OnSourcePathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(3);

            mode.OnSourceError(binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
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
                UpdateSource = () => throw new NotSupportedException(),
                UpdateTarget = () => ++updateCount,
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata =>
                    {
                        if (isAvailable)
                            return new MemberPathLastMember(this, ConstantMemberInfo.Target);
                        return default;
                    }
                }
            };

            IBindingSourceObserverListener mode = OneWayBindingMode.Instance;
            var oneTimeMode = OneWayBindingMode.OneTimeHandlerComponent.Instance;
            binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            binding.GetComponents<object>().ShouldContain(OneWayBindingMode.Instance, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnTargetLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                oneTimeMode.OnTargetPathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);

            updateCount.ShouldEqual(1);
            binding.GetComponents<object>().ShouldContain(OneWayBindingMode.Instance, oneTimeMode);

            isAvailable = true;
            oneTimeMode.OnTargetError(binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            updateCount.ShouldEqual(1);
            binding.GetComponents<object>().ShouldContain(OneWayBindingMode.Instance, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnTargetLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                oneTimeMode.OnTargetPathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(2);
        }

        #endregion
    }
}