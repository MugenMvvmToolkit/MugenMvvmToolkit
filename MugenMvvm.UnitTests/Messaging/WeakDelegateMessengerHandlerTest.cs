using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Messaging
{
    public class WeakDelegateMessengerHandlerTest : UnitTestBase
    {
        [Fact(Skip = ReleaseTest)]
        public void ShouldBeWeek1()
        {
            var sender = new object();
            var msg2 = "test";
            var messageContext2 = new MessageContext(sender, msg2, DefaultMetadata);

            var subscriber = ShouldBeWeekImpl1();
            GcCollect();
            subscriber.Handle(messageContext2).ShouldEqual(MessengerResult.Invalid);
        }

        [Fact(Skip = ReleaseTest)]
        public void ShouldBeWeek2()
        {
            var sender = new object();
            var msg2 = "test";
            var messageContext2 = new MessageContext(sender, msg2, DefaultMetadata);

            var subscriber = ShouldBeWeekImpl2();
            GcCollect();
            subscriber.Handle(messageContext2).ShouldEqual(MessengerResult.Invalid);
        }

        [Fact]
        public void ShouldHandleOnlySupportedTypes()
        {
            var subscriber = new WeakDelegateMessengerHandler<object, UnitTestBase>(Handler, WeakReferenceManager, ReflectionManager);
            subscriber.CanHandle(typeof(object)).ShouldBeFalse();
            subscriber.CanHandle(typeof(UnitTestBase)).ShouldBeTrue();
            subscriber.CanHandle(typeof(WeakDelegateMessengerHandlerTest)).ShouldBeTrue();
        }

        [Fact]
        public void ShouldInvokeDelegate1()
        {
            var sender = new object();
            var msg1 = new object();
            var msg2 = "test";

            var messageContext1 = new MessageContext(sender, msg1, DefaultMetadata);
            var messageContext2 = new MessageContext(sender, msg2, DefaultMetadata);

            var count = 0;
            var handler = new HandlerImpl
            {
                HandleFunc = (o, s, arg3) =>
                {
                    count++;
                    o.ShouldEqual(sender);
                    s.ShouldEqual(msg2);
                    arg3.ShouldEqual(messageContext2);
                }
            };

            var subscriber = new WeakDelegateMessengerHandler<HandlerImpl, string>(handler.Handle, WeakReferenceManager, ReflectionManager);
            subscriber.Handle(messageContext1).ShouldEqual(MessengerResult.Ignored);
            subscriber.Handle(messageContext2).ShouldEqual(MessengerResult.Handled);
            count.ShouldEqual(1);
        }

        [Fact]
        public void ShouldInvokeDelegate2()
        {
            var sender = new object();
            var msg1 = new object();
            var msg2 = "test";

            var messageContext1 = new MessageContext(sender, msg1, DefaultMetadata);
            var messageContext2 = new MessageContext(sender, msg2, DefaultMetadata);

            var count = 0;
            var handler = new HandlerImpl
            {
                HandleFunc = (o, s, arg3) =>
                {
                    count++;
                    o.ShouldEqual(sender);
                    s.ShouldEqual(msg2);
                    arg3.ShouldEqual(messageContext2);
                }
            };

            var subscriber = new WeakDelegateMessengerHandler<HandlerImpl, string>(handler, (impl, o, arg3, arg4) => impl.Handle(o, arg3, arg4), WeakReferenceManager);
            subscriber.Handle(messageContext1).ShouldEqual(MessengerResult.Ignored);
            subscriber.Handle(messageContext2).ShouldEqual(MessengerResult.Handled);
            count.ShouldEqual(1);
        }

        [Fact]
        public void ShouldValidateAnonymousMethod() =>
            ShouldThrow<NotSupportedException>(() => new WeakDelegateMessengerHandler<object, object>((_, _, _) => { }, WeakReferenceManager));

        [Fact]
        public void ShouldValidateArgs()
        {
            ShouldThrow<ArgumentNullException>(() => new WeakDelegateMessengerHandler<object, object>(null!, WeakReferenceManager));
            ShouldThrow<ArgumentNullException>(() => new WeakDelegateMessengerHandler<object, object>(new object(), null!, WeakReferenceManager));
            ShouldThrow<ArgumentNullException>(() => new WeakDelegateMessengerHandler<object, object>(null!, (_, _, _, _) => { }, WeakReferenceManager));
        }

        [Fact]
        public void ShouldValidateStaticMethod() => ShouldThrow<NotSupportedException>(() => new WeakDelegateMessengerHandler<object, object>(StaticMethod, WeakReferenceManager));

        private void Handler(object? arg1, object arg2, IMessageContext arg3)
        {
        }

        private static void StaticMethod(object? arg1, object arg2, IMessageContext arg3)
        {
        }

        private WeakDelegateMessengerHandler<HandlerImpl, string> ShouldBeWeekImpl1() => new(new HandlerImpl().Handle, WeakReferenceManager, ReflectionManager);

        private WeakDelegateMessengerHandler<HandlerImpl, string> ShouldBeWeekImpl2() => new(new HandlerImpl(), (_, _, _, _) => { }, WeakReferenceManager);

        private sealed class HandlerImpl
        {
            public Action<object?, string, IMessageContext>? HandleFunc { get; set; } = (o, s, arg3) => throw new NotSupportedException();

            public void Handle(object? arg1, string arg2, IMessageContext arg3) => HandleFunc!(arg1, arg2, arg3);
        }
    }
}