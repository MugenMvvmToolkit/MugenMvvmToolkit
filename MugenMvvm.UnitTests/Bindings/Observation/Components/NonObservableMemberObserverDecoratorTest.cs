using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class NonObservableMemberObserverDecoratorTest : UnitTestBase
    {
        [Fact]
        public void ShouldIgnoreNonObservableMembers()
        {
            var memberObserver = new MemberObserver((o, o1, arg3, arg4) => ActionToken.NoDoToken, this);
            var observationManager = new ObservationManager(ComponentCollectionManager);
            observationManager.AddComponent(new NonObservableMemberObserverDecorator());
            observationManager.AddComponent(new TestMemberObserverProviderComponent(observationManager)
            {
                TryGetMemberObserver = (_, _, _) => memberObserver
            });
            var member = new TestAccessorMemberInfo {MemberFlags = MemberFlags.Instance};

            observationManager.TryGetMemberObserver(typeof(object), member, DefaultMetadata).ShouldEqual(memberObserver);

            member.MemberFlags |= MemberFlags.NonObservable;
            observationManager.TryGetMemberObserver(typeof(object), member, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }
    }
}