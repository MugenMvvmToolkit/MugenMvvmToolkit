using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class NonObservableMemberObserverDecoratorTest : UnitTestBase
    {
        [Fact]
        public void ShouldIgnoreNonObservableMembers()
        {
            var memberObserver = new MemberObserver((o, o1, arg3, arg4) => ActionToken.NoDo, this);
            ObservationManager.AddComponent(new NonObservableMemberObserverDecorator());
            ObservationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (_, _, _, _) => memberObserver
            });
            var member = new TestAccessorMemberInfo { MemberFlags = MemberFlags.Instance };

            ObservationManager.TryGetMemberObserver(typeof(object), member, DefaultMetadata).ShouldEqual(memberObserver);

            member.MemberFlags |= MemberFlags.NonObservable;
            ObservationManager.TryGetMemberObserver(typeof(object), member, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }
    }
}