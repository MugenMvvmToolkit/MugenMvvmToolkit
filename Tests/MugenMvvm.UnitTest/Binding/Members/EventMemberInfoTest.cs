using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers;
using MugenMvvm.UnitTest.Binding.Observers;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class EventMemberInfoTest : UnitTestBase
    {
        #region Events

        public event Action? Event1;

        public event Action<object>? Event2;

        #endregion

        #region Methods

        [Theory]
        [InlineData(nameof(Event1))]
        [InlineData(nameof(Event2))]
        public void ConstructorShouldInitializeMember(string eventName)
        {
            var eventInfo = GetType().GetEvent(eventName);
            eventInfo.ShouldNotBeNull();
            var name = eventName + "t";
            var testEventListener = new TestEventListener();
            var result = new ActionToken((o, o1) => { });
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
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Instance);

            memberInfo.TrySubscribe(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
        }

        #endregion
    }
}