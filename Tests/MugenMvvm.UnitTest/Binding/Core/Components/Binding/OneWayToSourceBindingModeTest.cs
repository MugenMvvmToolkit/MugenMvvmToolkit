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
    public class OneWayToSourceBindingModeTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldUpdateBindingIfTargetAvailable()
        {
            var updateCount = 0;
            var binding = new TestBinding
            {
                UpdateTarget = () => throw new NotSupportedException(),
                UpdateSource = () => ++updateCount,
                Source = new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                }
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
                Source = new TestMemberPathObserver
                {
                    GetLastMember = metadata =>
                    {
                        if (isAvailable)
                            return new MemberPathLastMember(this, ConstantMemberInfo.Target);
                        return default;
                    }
                }
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

        #endregion
    }
}