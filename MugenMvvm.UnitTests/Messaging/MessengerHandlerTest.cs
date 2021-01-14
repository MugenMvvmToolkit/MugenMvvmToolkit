using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Messaging
{
    public class MessengerHandlerTest : UnitTestBase
    {
        [Fact]
        public void HandleShouldReturnIgnoredDefault()
        {
            MessengerHandler handler = default;
            handler.Handle(new MessageContext(new object(), new object(), DefaultMetadata)).ShouldEqual(MessengerResult.Ignored);
        }

        [Fact]
        public void HandleShouldUseDelegate()
        {
            var messageContext = new MessageContext(new object(), new object(), DefaultMetadata);
            var subscriber = new object();
            var executionMode = ThreadExecutionMode.MainAsync;
            var state = new object();
            var invokeCount = 0;
            var result = MessengerResult.Ignored;
            Func<object, IMessageContext, object?, MessengerResult> handler = (o, arg3, o1) =>
            {
                ++invokeCount;
                o.ShouldEqual(subscriber);
                o1.ShouldEqual(state);
                arg3.ShouldEqual(messageContext);
                return result;
            };
            var messengerHandler = new MessengerHandler(handler, subscriber, executionMode, state);
            messengerHandler.Deconstruct(out var s, out var m, out var h, out var st);
            s.ShouldEqual(subscriber);
            m.ShouldEqual(executionMode);
            h.ShouldEqual(handler);
            st.ShouldEqual(state);
            messengerHandler.ExecutionMode.ShouldEqual(executionMode);
            messengerHandler.Subscriber.ShouldEqual(subscriber);

            messengerHandler.Handle(messageContext).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            result = MessengerResult.Handled;
            messengerHandler.Handle(messageContext).ShouldEqual(result);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void IsEmptyShouldReturnTrueForDefault()
        {
            MessengerHandler handler = default;
            handler.IsEmpty.ShouldBeTrue();
        }
    }
}