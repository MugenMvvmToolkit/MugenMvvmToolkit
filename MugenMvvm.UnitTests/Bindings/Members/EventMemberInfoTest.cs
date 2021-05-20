#pragma warning disable CS0067
using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class EventMemberInfoTest : UnitTestBase
    {
        public event Action? Event1;

        public event Action<object>? Event2;

        [Theory]
        [InlineData(nameof(Event1))]
        [InlineData(nameof(Event2))]
        public void ConstructorShouldInitializeMember(string eventName)
        {
            var eventInfo = GetType().GetEvent(eventName)!;
            eventInfo.ShouldNotBeNull();
            var name = eventName + "t";
            var testEventListener = new TestWeakEventListener();
            var result = ActionToken.FromDelegate((o, o1) => { });
            var count = 0;

            var memberObserver = new MemberObserver((target, member, listener, meta) =>
            {
                ++count;
                target.ShouldEqual(this);
                member.ShouldEqual(eventInfo);
                listener.ShouldEqual(testEventListener);
                meta.ShouldEqual(DefaultMetadata);
                return result;
            }, eventInfo);
            var memberInfo = new EventMemberInfo(name, eventInfo, memberObserver);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.Type.ShouldEqual(eventInfo.EventHandlerType);
            memberInfo.DeclaringType.ShouldEqual(eventInfo.DeclaringType);
            memberInfo.UnderlyingMember.ShouldEqual(eventInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Event);
            memberInfo.MemberFlags.ShouldEqual(MemberFlags.Public | MemberFlags.Instance);

            memberInfo.TryObserve(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
        }
    }
}