using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Messaging;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Infrastructure.Messaging
{
    public class DelegateMessengerSubscriberTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void SubscriberShouldValidateArgsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateMessengerSubscriber<object>(null!));
        }

        [Fact]
        public void SubscriberShouldAllowDuplicates()
        {
            Action<object, object, IMessengerContext> action = (o, o1, arg3) => { };
            var hashSet = new HashSet<object>();
            var subscriber1 = new DelegateMessengerSubscriber<string>(action);
            var subscriber2 = new DelegateMessengerSubscriber<string>(action);
            hashSet.Add(subscriber1).ShouldBeTrue();
            hashSet.Add(subscriber2).ShouldBeTrue();
        }

        [Fact]
        public void SubscriberShouldInvokeDelegate()
        {
            var sender = new object();
            var msg1 = new object();
            var msg2 = "test";

            var count = 0;
            var subscriber = new DelegateMessengerSubscriber<string>((o, s, arg3) =>
            {
                count++;
                o.ShouldEqual(sender);
                s.ShouldEqual(msg2);
                arg3.ShouldBeNull();
            });
            subscriber.Handle(sender, msg1, null!);
            subscriber.Handle(sender, msg2, null!);
            count.ShouldEqual(1);
        }

        [Fact]
        public void SubscriberShouldHandleOnlyValidMessageType()
        {
            var sender = new object();
            var msg1 = new object();
            var msg2 = "test";

            var subscriber = new DelegateMessengerSubscriber<string>((o, s, arg3) => { });
            subscriber.Handle(sender, msg1, null!).ShouldEqual(MessengerSubscriberResult.Ignored);
            subscriber.Handle(sender, msg2, null!).ShouldEqual(MessengerSubscriberResult.Handled);
        }

        #endregion
    }
}