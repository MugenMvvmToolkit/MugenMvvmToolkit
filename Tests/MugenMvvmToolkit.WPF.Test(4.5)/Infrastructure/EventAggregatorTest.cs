using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models.Messages;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure
{
    public class Alpha
    {
    }

    public class Beta : Alpha
    {
    }

    public class AlphaBetaHandler : IHandler<Alpha>, IHandler<Beta>
    {
        #region Properties

        public Alpha Alpha { get; set; }

        public Beta Beta { get; set; }

        public int CountAlfa { get; set; }

        public int CountBeta { get; set; }

        #endregion

        #region Implementation of IHandler<in Alpha>

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        public void Handle(object sender, Alpha message)
        {
            Alpha = message;
            CountAlfa++;
        }

        #endregion

        #region Implementation of IHandler<in Beta>

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        public void Handle(object sender, Beta message)
        {
            Beta = message;
            CountBeta++;
        }

        #endregion
    }

    public class GenericHandler<T> : IHandler<T>
    {
        #region Properties

        public int Count;

        public T Message;

        public object Sender;

        #endregion

        #region Implementation of IHandler<in T>

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        public void Handle(object sender, T message)
        {
            Sender = sender;
            Message = message;
            Count++;
        }

        #endregion
    }

    public class TestObservable : IObservable
    {
        #region Fields

        public IEventAggregator Listeners { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestObservable" /> class.
        /// </summary>
        public TestObservable()
        {
            Listeners = new EventAggregator();
        }

        #endregion

        #region Implementation of IObservable

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="instance">The instance to subscribe for event publication.</param>
        public bool Subscribe(ISubscriber instance)
        {
            if (Listeners == null)
                return false;
            return Listeners.Subscribe(instance);
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="instance">The instance to unsubscribe.</param>
        public bool Unsubscribe(ISubscriber instance)
        {
            if (Listeners != null)
                return Listeners.Unsubscribe(instance);
            return false;
        }

        #endregion
    }

    [TestClass]
    public class EventAggregatorTest : TestBase
    {
        #region Methods

        [TestMethod]
        public void AddInvalidListenerTest()
        {
            IEventAggregator eventAggregator = CreateEventAggregator();
            eventAggregator.Subscribe(new object()).ShouldBeNull();
            eventAggregator.GetSubscribers().ShouldBeEmpty();
        }

        [TestMethod]
        public void AddWeakListenerTest()
        {
            var listener = new GenericHandler<object>();
            IEventAggregator eventAggregator = CreateEventAggregator();
            var subscriber = eventAggregator.Subscribe(listener);
            eventAggregator.Contains(subscriber).ShouldBeTrue();
            eventAggregator.GetSubscribers().Count.ShouldEqual(1);

            listener = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            eventAggregator.GetSubscribers().Count.ShouldEqual(0);
        }

        [TestMethod]
        public void AddWeakContainerListenerTest()
        {
            var listener = new TestObservable();
            IEventAggregator eventAggregator = CreateEventAggregator();
            eventAggregator.Subscribe(listener);
            eventAggregator.GetSubscribers().Count.ShouldEqual(0);
        }

        [TestMethod]
        public void RemoveInvalidListenerTest()
        {
            IEventAggregator eventAggregator = CreateEventAggregator();
            eventAggregator.Unsubscribe(new object()).ShouldBeFalse();
        }

        [TestMethod]
        public void RemoveWeakListenerTest()
        {
            var listener = new GenericHandler<object>();
            IEventAggregator eventAggregator = CreateEventAggregator();
            var subscriber = eventAggregator.Subscribe(listener);
            eventAggregator.Contains(subscriber).ShouldBeTrue();
            eventAggregator.GetSubscribers().Count.ShouldEqual(1);

            eventAggregator.Unsubscribe(listener).ShouldBeTrue();
            eventAggregator.GetSubscribers().Count.ShouldEqual(0);
        }

        [TestMethod]
        public void ClearTest()
        {
            var listener = new GenericHandler<object>();
            IEventAggregator eventAggregator = CreateEventAggregator();
            var subscriber = eventAggregator.Subscribe(listener);
            eventAggregator.Contains(subscriber).ShouldBeTrue();
            eventAggregator.GetSubscribers().Count.ShouldEqual(1);

            eventAggregator.UnsubscribeAll();
            eventAggregator.GetSubscribers().Count.ShouldEqual(0);
        }

        [TestMethod]
        public void NotifyOneListenerTest()
        {
            var listener = new GenericHandler<object>();
            IEventAggregator eventAggregator = CreateEventAggregator();
            eventAggregator.Subscribe(listener);

            eventAggregator.Publish(this, eventAggregator);
            listener.Count.ShouldEqual(1);
            listener.Sender.ShouldEqual(this);
            listener.Message.ShouldEqual(eventAggregator);
        }

        [TestMethod]
        public void NotifyListenerWithContainerTest()
        {
            var listener = new GenericHandler<object>();
            var containerListener = new GenericHandler<object>();
            var stateChanged = new TestObservable();
            stateChanged.Subscribe(containerListener);

            IEventAggregator eventAggregator = CreateEventAggregator();
            eventAggregator.Subscribe(listener);
            eventAggregator.Subscribe(stateChanged);
            eventAggregator.Publish(this, eventAggregator);
            listener.Count.ShouldEqual(1);
            listener.Sender.ShouldEqual(this);
            listener.Message.ShouldEqual(eventAggregator);

            containerListener.Count.ShouldEqual(0);
            containerListener.Sender.ShouldBeNull();
            containerListener.Message.ShouldBeNull();
        }

        [TestMethod]
        public void NotifyListenerWithEmptyContainerTest()
        {
            var listener = new GenericHandler<object>();
            var observable = new TestObservable { Listeners = null };

            IEventAggregator eventAggregator = CreateEventAggregator();
            eventAggregator.Subscribe(listener);
            eventAggregator.Subscribe(observable);
            eventAggregator.Publish(this, eventAggregator);
            listener.Count.ShouldEqual(1);
            listener.Sender.ShouldEqual(this);
            listener.Message.ShouldEqual(eventAggregator);
        }

        [TestMethod]
        public void NotifySelfShouldNotThrowExceptionTest()
        {
            var listener = new GenericHandler<object>();
            var containerListener = new GenericHandler<object>();
            var observable = new TestObservable();
            observable.Subscribe(containerListener);

            IEventAggregator eventAggregator = CreateEventAggregator();
            eventAggregator.Subscribe(listener);
            eventAggregator.Subscribe(observable);
            observable.Subscribe(new TestObservable { Listeners = eventAggregator });

            eventAggregator.Publish(this, eventAggregator);
            listener.Count.ShouldEqual(1);
            listener.Sender.ShouldEqual(this);
            listener.Message.ShouldEqual(eventAggregator);

            containerListener.Count.ShouldEqual(0);
            containerListener.Sender.ShouldBeNull();
            containerListener.Message.ShouldBeNull();
        }

        [TestMethod]
        public void NotifyGenericMessageTest()
        {
            var listenerAlfa = new GenericHandler<Alpha>();
            var listenerBeta = new GenericHandler<Beta>();
            IEventAggregator eventAggregator = CreateEventAggregator();
            eventAggregator.Subscribe(listenerAlfa);
            eventAggregator.Subscribe(listenerBeta);

            //Publish about alfa.
            var alfa = new Alpha();
            eventAggregator.Publish(eventAggregator, alfa);
            listenerAlfa.Count.ShouldEqual(1);
            listenerAlfa.Message.ShouldEqual(alfa);
            listenerAlfa.Sender.ShouldEqual(eventAggregator);
            listenerBeta.Count.ShouldEqual(0);

            //Publish about beta.
            var beta = new Beta();
            eventAggregator.Publish(eventAggregator, beta);
            listenerAlfa.Count.ShouldEqual(2);
            listenerAlfa.Message.ShouldEqual(beta);
            listenerAlfa.Sender.ShouldEqual(eventAggregator);
            listenerBeta.Count.ShouldEqual(1);
            listenerBeta.Message.ShouldEqual(beta);
            listenerBeta.Sender.ShouldEqual(eventAggregator);
        }

        [TestMethod]
        public void NotifyTwoListenerWithBaseClassTest()
        {
            var listener = new AlphaBetaHandler();
            IEventAggregator eventAggregator = CreateEventAggregator();
            eventAggregator.Subscribe(listener);

            //Publish about alfa.
            var alfa = new Alpha();
            eventAggregator.Publish(eventAggregator, alfa);
            listener.CountAlfa.ShouldEqual(1);
            listener.Alpha.ShouldEqual(alfa);
            listener.CountBeta.ShouldEqual(0);

            //Publish about beta.
            var beta = new Beta();
            eventAggregator.Publish(eventAggregator, beta);
            listener.CountAlfa.ShouldEqual(2);
            listener.Alpha.ShouldEqual(beta);
            listener.CountBeta.ShouldEqual(1);
            listener.Beta.ShouldEqual(beta);
        }

        [TestMethod]
        public void CycleHandleTest()
        {
            IEventAggregator eventAggregator1 = CreateEventAggregator();
            IEventAggregator eventAggregator2 = CreateEventAggregator();
            eventAggregator1.Subscribe(eventAggregator2).ShouldNotBeNull();
            eventAggregator2.Subscribe(eventAggregator1).ShouldNotBeNull();

            eventAggregator1.Publish(eventAggregator1, StateChangedMessage.Empty);
        }

        [TestMethod]
        public void NonWeakLambdaMethodShouldBeSupported()
        {
            var message = new object();
            IEventAggregator eventAggregator = CreateEventAggregator();
            bool isInvoked = false;
            var subscriber = eventAggregator.Subscribe<object>((arg1, arg2) =>
             {
                 arg1.ShouldEqual(eventAggregator);
                 arg2.ShouldEqual(message);
                 isInvoked = true;
             }, false);
            eventAggregator.Contains(subscriber).ShouldBeTrue();
            eventAggregator.GetSubscribers().Count.ShouldEqual(1);
            eventAggregator.Publish(eventAggregator, message);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void WeakLambdaMethodShouldNotBeSupported()
        {
            IEventAggregator eventAggregator = CreateEventAggregator();
            ShouldThrow<NotSupportedException>(() => eventAggregator.Subscribe<object>((arg1, arg2) => arg1.ShouldEqual(eventAggregator), true));
        }

        [TestMethod]
        public void WeakMethodShouldBeSupported()
        {
            var message = new object();
            var listener = new GenericHandler<object>();
            IEventAggregator eventAggregator = CreateEventAggregator();
            var subscriber = eventAggregator.Subscribe<object>(listener.Handle);
            eventAggregator.Contains(subscriber).ShouldBeTrue();
            eventAggregator.GetSubscribers().Count.ShouldEqual(1);

            eventAggregator.Publish(eventAggregator, message);
            listener.Sender.ShouldEqual(eventAggregator);
            listener.Message.ShouldEqual(message);
            listener.Count.ShouldEqual(1);

            listener = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            eventAggregator.GetSubscribers().ShouldBeEmpty();
        }

        [TestMethod]
        public void InstanceNonWeakMethodShouldBeSupported()
        {
            var message = new object();
            var listener = new GenericHandler<object>();
            IEventAggregator eventAggregator = CreateEventAggregator();
            var subscriber = eventAggregator.Subscribe<object>(listener.Handle, false);
            eventAggregator.Contains(subscriber).ShouldBeTrue();
            eventAggregator.GetSubscribers().Count.ShouldEqual(1);

            eventAggregator.Publish(eventAggregator, message);
            listener.Sender.ShouldEqual(eventAggregator);
            listener.Message.ShouldEqual(message);
            listener.Count.ShouldEqual(1);

            listener = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            eventAggregator.Contains(subscriber).ShouldBeTrue();
        }

        [TestMethod]
        public void WeakMethodUnsubscribeShouldBeSupported()
        {
            var listener = new GenericHandler<object>();
            IEventAggregator eventAggregator = CreateEventAggregator();
            var subscriber = eventAggregator.Subscribe<object>(listener.Handle);
            eventAggregator.Contains(subscriber).ShouldBeTrue();
            eventAggregator.GetSubscribers().Count.ShouldEqual(1);

            eventAggregator.Unsubscribe<object>(listener.Handle);
            eventAggregator.GetSubscribers().ShouldBeEmpty();
        }

        [TestMethod]
        public void InstanceMethodUnsubscribeShouldBeSupported()
        {
            var listener = new GenericHandler<object>();
            IEventAggregator eventAggregator = CreateEventAggregator();
            var subscriber = eventAggregator.Subscribe<object>(listener.Handle, false);
            eventAggregator.Contains(subscriber).ShouldBeTrue();
            eventAggregator.GetSubscribers().Count.ShouldEqual(1);

            eventAggregator.Unsubscribe<object>(listener.Handle);
            eventAggregator.GetSubscribers().ShouldBeEmpty();
        }

        protected virtual IEventAggregator CreateEventAggregator()
        {
            return new EventAggregator(true);
        }

        #endregion
    }
}