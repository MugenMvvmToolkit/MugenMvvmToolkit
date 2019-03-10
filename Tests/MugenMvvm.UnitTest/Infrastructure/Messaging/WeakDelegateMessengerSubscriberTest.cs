//using System;
//using System.Collections.Generic;
//using MugenMvvm.Enums;
//using MugenMvvm.Infrastructure.Messaging;
//using MugenMvvm.Interfaces.Messaging;
//using Should;
//using Xunit;
//
//namespace MugenMvvm.UnitTest.Infrastructure.Messaging
//{
//    public class WeakDelegateMessengerSubscriberTest : UnitTestBase
//    {
//        #region Methods
//
//        [Fact]
//        public void SubscriberShouldValidateArgsNull()
//        {
//            Assert.Throws<ArgumentNullException>(() => new WeakDelegateMessengerSubscriber<object, object>(null!));
//            Assert.Throws<ArgumentNullException>(() => new WeakDelegateMessengerSubscriber<object, object>(new object(), null!));
//            Assert.Throws<ArgumentNullException>(() => new WeakDelegateMessengerSubscriber<object, object>(null!, (o, o1, arg3, arg4) => { }));
//        }
//
//        [Fact]
//        public void SubscriberShouldValidateStaticMethod()
//        {
//            Assert.Throws<NotSupportedException>(() => new WeakDelegateMessengerSubscriber<object, object>(StaticMethod));
//        }
//
//        [Fact]
//        public void SubscriberShouldValidateAnonymousMethod()
//        {
//            Assert.Throws<NotSupportedException>(() => new WeakDelegateMessengerSubscriber<object, object>((o, o1, arg3) => { }));
//        }
//
//        [Fact]
//        public void SubscriberShouldAllowDuplicates()
//        {
//            Action<object, object, IMessengerContext> action = Handler;
//            var hashSet = new HashSet<object>();
//            var subscriber1 = new WeakDelegateMessengerSubscriber<object, object>(action);
//            var subscriber2 = new WeakDelegateMessengerSubscriber<object, object>(action);
//            hashSet.Add(subscriber1).ShouldBeTrue();
//            hashSet.Add(subscriber2).ShouldBeTrue();
//        }
//
//        [Fact]
//        public void SubscriberShouldInvokeDelegate1()
//        {
//            var sender = new object();
//            var msg1 = new object();
//            var msg2 = "test";
//
//            var count = 0;
//            var handler = new HandlerImpl
//            {
//                HandleFunc = (o, s, arg3) =>
//                {
//                    count++;
//                    o.ShouldEqual(sender);
//                    s.ShouldEqual(msg2);
//                    arg3.ShouldBeNull();
//                }
//            };
//
//            var subscriber = new WeakDelegateMessengerSubscriber<HandlerImpl, string>(handler.Handle);
//            subscriber.Handle(sender, msg1, null!);
//            subscriber.Handle(sender, msg2, null!);
//            count.ShouldEqual(1);
//        }
//
//        [Fact]
//        public void SubscriberShouldInvokeDelegate2()
//        {
//            var sender = new object();
//            var msg1 = new object();
//            var msg2 = "test";
//
//            var count = 0;
//            var handler = new HandlerImpl
//            {
//                HandleFunc = (o, s, arg3) =>
//                {
//                    count++;
//                    o.ShouldEqual(sender);
//                    s.ShouldEqual(msg2);
//                    arg3.ShouldBeNull();
//                }
//            };
//
//            var subscriber = new WeakDelegateMessengerSubscriber<HandlerImpl, string>(handler, (impl, o, arg3, arg4) => impl.Handle(o, arg3, arg4));
//            subscriber.Handle(sender, msg1, null!);
//            subscriber.Handle(sender, msg2, null!);
//            count.ShouldEqual(1);
//        }
//
//        [Fact]
//        public void SubscriberShouldHandleOnlyValidMessageType1()
//        {
//            var sender = new object();
//            var msg1 = new object();
//            var msg2 = "test";
//            var handler = new HandlerImpl
//            {
//                HandleFunc = (o, s, arg3) =>
//                {
//
//                }
//            };
//
//            var subscriber = new WeakDelegateMessengerSubscriber<HandlerImpl, string>(handler.Handle);
//            subscriber.Handle(sender, msg1, null!).ShouldEqual(MessengerSubscriberResult.Ignored);
//            subscriber.Handle(sender, msg2, null!).ShouldEqual(MessengerSubscriberResult.Handled);
//        }
//
//        [Fact]
//        public void SubscriberShouldHandleOnlyValidMessageType2()
//        {
//            var sender = new object();
//            var msg1 = new object();
//            var msg2 = "test";
//            var handler = new HandlerImpl
//            {
//                HandleFunc = (o, s, arg3) =>
//                {
//
//                }
//            };
//
//            var subscriber = new WeakDelegateMessengerSubscriber<HandlerImpl, string>(handler, (impl, o, arg3, arg4) => impl.Handle(o, arg3, arg4));
//            subscriber.Handle(sender, msg1, null!).ShouldEqual(MessengerSubscriberResult.Ignored);
//            subscriber.Handle(sender, msg2, null!).ShouldEqual(MessengerSubscriberResult.Handled);
//        }
//
//#if !DEBUG
//        [Fact]
//        public void SubscriberShouldBeWeak1()
//        {
//            var sender = new object();
//            var msg = "test";
//
//            var handler = new HandlerImpl
//            {
//                HandleFunc = (o, s, arg3) =>
//                {
//
//                }
//            };
//            var subscriber = new WeakDelegateMessengerSubscriber<HandlerImpl, string>(handler.Handle);
//            subscriber.Handle(sender, msg, null!).ShouldEqual(SubscriberResult.Handled);
//
//            handler = null;
//            GC.Collect();
//            GC.WaitForPendingFinalizers();
//            GC.Collect();
//
//            subscriber.Handle(sender, msg, null!).ShouldEqual(SubscriberResult.Invalid);
//        }
//
//        [Fact]
//        public void SubscriberShouldBeWeak2()
//        {
//            var sender = new object();
//            var msg = "test";
//
//            var handler = new HandlerImpl
//            {
//                HandleFunc = (o, s, arg3) =>
//                {
//
//                }
//            };
//            var subscriber = new WeakDelegateMessengerSubscriber<HandlerImpl, string>(handler, (impl, o, arg3, arg4) => impl.Handle(o, arg3, arg4));
//            subscriber.Handle(sender, msg, null!).ShouldEqual(SubscriberResult.Handled);
//
//            handler = null;
//            GC.Collect();
//            GC.WaitForPendingFinalizers();
//            GC.Collect();
//
//            subscriber.Handle(sender, msg, null!).ShouldEqual(SubscriberResult.Invalid);
//        }
//#endif
//
//        private void Handler(object arg1, object arg2, IMessengerContext arg3)
//        {
//        }
//
//        private static void StaticMethod(object arg1, object arg2, IMessengerContext arg3)
//        {
//        }
//
//        #endregion
//
//        #region Nested types
//
//        private sealed class HandlerImpl
//        {
//            #region Properties
//
//            public Action<object, string, IMessengerContext> HandleFunc { get; set; }
//
//            #endregion
//
//            #region Methods
//
//            public void Handle(object arg1, string arg2, IMessengerContext arg3)
//            {
//                HandleFunc(arg1, arg2, arg3);
//            }
//
//            #endregion
//        }
//
//        #endregion
//    }
//}