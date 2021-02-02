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
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class OneWayToSourceBindingModeTest : UnitTestBase
    {
        private readonly TestBinding _binding;

        public OneWayToSourceBindingModeTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _binding = new TestBinding(ComponentCollectionManager);
        }

        [Fact]
        public void ShouldUpdateBindingIfTargetAvailable()
        {
            var updateCount = 0;

            _binding.UpdateTarget = () => throw new NotSupportedException();
            _binding.UpdateSource = () => ++updateCount;
            _binding.Source = ItemOrArray.FromItem<object?>(new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
            });

            IBindingTargetObserverListener mode = OneWayToSourceBindingMode.Instance;
            _binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            _binding.GetComponents<object>().Single().ShouldEqual(mode);

            mode.OnTargetLastMemberChanged(_binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(2);

            mode.OnTargetPathMembersChanged(_binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(3);

            mode.OnTargetError(_binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            updateCount.ShouldEqual(3);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldUpdateBindingIfNotAvailable(bool lastMember)
        {
            var updateCount = 0;
            var isAvailable = false;

            _binding.UpdateTarget = () => throw new NotSupportedException();
            _binding.UpdateSource = () => ++updateCount;
            _binding.Source = ItemOrArray.FromItem<object?>(new TestMemberPathObserver
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
            _binding.AddComponent(mode);
            updateCount.ShouldEqual(1);
            _binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnSourceLastMemberChanged(_binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                oneTimeMode.OnSourcePathMembersChanged(_binding, EmptyPathObserver.Empty, DefaultMetadata);

            updateCount.ShouldEqual(1);
            _binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            isAvailable = true;
            oneTimeMode.OnSourceError(_binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            updateCount.ShouldEqual(1);
            _binding.GetComponents<object>().ShouldContain(mode, oneTimeMode);

            if (lastMember)
                oneTimeMode.OnSourceLastMemberChanged(_binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                oneTimeMode.OnSourcePathMembersChanged(_binding, EmptyPathObserver.Empty, DefaultMetadata);
            updateCount.ShouldEqual(2);
        }
    }
}