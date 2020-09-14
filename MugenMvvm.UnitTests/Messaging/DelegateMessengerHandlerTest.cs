using MugenMvvm.Enums;
using MugenMvvm.Messaging;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Messaging
{
    public class DelegateMessengerHandlerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldHandleOnlySupportedTypes()
        {
            var subscriber = new DelegateMessengerHandler<UnitTestBase>((o, s, arg3) => { });
            subscriber.CanHandle(typeof(object)).ShouldBeFalse();
            subscriber.CanHandle(typeof(UnitTestBase)).ShouldBeTrue();
            subscriber.CanHandle(typeof(DelegateMessengerHandlerTest)).ShouldBeTrue();
        }

        [Fact]
        public void ShouldInvokeDelegate()
        {
            var sender = new object();
            var msg1 = new object();
            var msg2 = "test";
            var messageContext1 = new MessageContext(sender, msg1, DefaultMetadata);
            var messageContext2 = new MessageContext(sender, msg2, DefaultMetadata);

            var count = 0;
            var subscriber = new DelegateMessengerHandler<string>((o, s, arg3) =>
            {
                count++;
                o.ShouldEqual(sender);
                s.ShouldEqual(msg2);
                arg3.ShouldEqual(messageContext2);
            });
            subscriber.Handle(messageContext1).ShouldEqual(MessengerResult.Ignored);
            subscriber.Handle(messageContext2).ShouldEqual(MessengerResult.Handled);
            count.ShouldEqual(1);
        }

        #endregion
    }
}