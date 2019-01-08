using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Messaging;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Infrastructure.Messaging
{
    public class MessengerHandlerSubscriberTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void SubscriberShouldValidateArgsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MessengerHandlerSubscriber(null!));
        }

        [Fact]
        public void SubscriberShouldNotAllowDuplicates()
        {
            var handler = new TestMessageHandler();
            var hashSet = new HashSet<object>();
            var subscriber1 = new MessengerHandlerSubscriber(handler);
            var subscriber2 = new MessengerHandlerSubscriber(handler);
            hashSet.Add(subscriber1).ShouldBeTrue();
            hashSet.Add(subscriber2).ShouldBeFalse();
        }

        [Fact]
        public void SubscriberShouldHandleOnlyValidMessageType()
        {
            var sender = new object();
            var msg1 = new object();
            var msg2 = "test";

            var subscriber = new MessengerHandlerSubscriber(new StringHandler());
            subscriber.Handle(sender, msg1, null!).ShouldEqual(SubscriberResult.Ignored);
            subscriber.Handle(sender, msg2, null!).ShouldEqual(SubscriberResult.Handled);
        }

        [Fact]
        public void SubscriberShouldCallCorrectHandler()
        {
            var sender = new object();
            var objMsg = new object();
            var stringMsg = "test";
            var intMsg = 1;
            var decimalMsg = 1M;

            int invokeCount = 0;
            var handler = new TestMessageHandler();
            handler.IntHandle = (o, i, arg3) => throw new NotSupportedException();
            handler.StringHandle = (o, i, arg3) => throw new NotSupportedException();
            handler.ObjectHandle = (o, i, arg3) =>
            {
                o.ShouldEqual(sender);
                i.ShouldEqual(objMsg);
                ++invokeCount;
            };

            var subscriber = new MessengerHandlerSubscriber(handler);
            subscriber.Handle(sender, objMsg, null!).ShouldEqual(SubscriberResult.Handled);
            invokeCount.ShouldEqual(1);

            handler.ObjectHandle = (o, i, arg3) => throw new NotSupportedException();
            handler.StringHandle = (o, i, arg3) => throw new NotSupportedException();
            handler.IntHandle = (o, i, arg3) =>
            {
                o.ShouldEqual(sender);
                i.ShouldEqual(intMsg);
                ++invokeCount;
            };
            subscriber.Handle(sender, intMsg, null!).ShouldEqual(SubscriberResult.Handled);
            invokeCount.ShouldEqual(2);

            handler.ObjectHandle = (o, i, arg3) => throw new NotSupportedException();
            handler.IntHandle = (o, i, arg3) => throw new NotSupportedException();
            handler.StringHandle = (o, i, arg3) =>
            {
                o.ShouldEqual(sender);
                i.ShouldEqual(stringMsg);
                ++invokeCount;
            };
            subscriber.Handle(sender, stringMsg, null!).ShouldEqual(SubscriberResult.Handled);
            invokeCount.ShouldEqual(3);

            handler.ObjectHandle = (o, i, arg3) => throw new NotSupportedException();
            handler.IntHandle = (o, i, arg3) => throw new NotSupportedException();
            handler.StringHandle = (o, i, arg3) => throw new NotSupportedException();
            subscriber.Handle(sender, decimalMsg, null!).ShouldEqual(SubscriberResult.Ignored);
        }

#if !DEBUG
        [Fact]
        public void SubscriberShouldBeWeak()
        {
            var sender = new object();
            var msg = "test";

            var handler = new StringHandler();
            var subscriber = new MessengerHandlerSubscriber(handler);
            subscriber.Handle(sender, msg, null!).ShouldEqual(SubscriberResult.Handled);

            handler = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            subscriber.Handle(sender, msg, null!).ShouldEqual(SubscriberResult.Invalid);
        }
#endif

        #endregion

        #region Nested types

        private sealed class StringHandler : IMessengerHandler<string>
        {
            #region Implementation of interfaces

            public void Handle(object sender, string message, IMessengerContext messengerContext)
            {
            }

            #endregion
        }

        private sealed class TestMessageHandler : IMessengerHandler<object>, IMessengerHandler<int>, IMessengerHandler<string>
        {
            #region Properties

            public Action<object, int, IMessengerContext> IntHandle { get; set; }

            public Action<object, object, IMessengerContext> ObjectHandle { get; set; }

            public Action<object, string, IMessengerContext> StringHandle { get; set; }

            #endregion

            #region Implementation of interfaces

            void IMessengerHandler<int>.Handle(object sender, int message, IMessengerContext messengerContext)
            {
                IntHandle(sender, message, messengerContext);
            }

            void IMessengerHandler<object>.Handle(object sender, object message, IMessengerContext messengerContext)
            {
                ObjectHandle(sender, message, messengerContext);
            }

            void IMessengerHandler<string>.Handle(object sender, string message, IMessengerContext messengerContext)
            {
                StringHandle(sender, message, messengerContext);
            }

            #endregion
        }

        #endregion
    }
}