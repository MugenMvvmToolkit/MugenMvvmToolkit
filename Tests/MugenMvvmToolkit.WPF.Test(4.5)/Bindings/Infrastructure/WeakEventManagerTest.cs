using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Infrastructure
{
    [TestClass]
    public class WeakEventManagerTest : BindingTestBase
    {
        #region Methods

        [TestMethod]
        public void WeakEventManagerShouldSubscribeAndUnsubscribePropertyChangedEvent()
        {
            const string propertyName = "test";
            int invokedCount = 0;
            var model = new BindingSourceModel();
            var listenerMock = new EventListenerMock();
            IWeakEventManager weakEventManager = CreateWeakEventManager();
            var disposable = weakEventManager.Subscribe(model, propertyName, listenerMock);

            listenerMock.Handle = (o, o1) => invokedCount++;
            invokedCount.ShouldEqual(0);

            model.OnPropertyChanged(propertyName + "1");
            invokedCount.ShouldEqual(0);
            model.OnPropertyChanged(propertyName);
            invokedCount.ShouldEqual(1);

            disposable.Dispose();

            model.OnPropertyChanged(propertyName);
            invokedCount.ShouldEqual(1);
        }

        [TestMethod]
        public void WeakEventManagerShouldSubscribeAndUnsubscribePropertyChangedEventSeveralSources()
        {
            const int count = 100;
            const string propertyName = "test";

            var model = new BindingSourceModel();
            var listeners = new List<EventListenerMock>();
            var invokedItems = new List<EventListenerMock>();
            IWeakEventManager weakEventManager = CreateWeakEventManager();

            for (int i = 0; i < count; i++)
            {
                var listenerMock = new EventListenerMock();
                var disposable = weakEventManager.Subscribe(model, propertyName, listenerMock);
                listeners.Add(listenerMock);
                listenerMock.Handle = (o, o1) =>
                {
                    invokedItems.Add(listenerMock);
                    disposable.Dispose();
                };
            }
            model.OnPropertyChanged(propertyName + "1");
            model.OnPropertyChanged(propertyName);
            model.OnPropertyChanged(propertyName);

            invokedItems.Count.ShouldEqual(count);
            foreach (var listener in listeners)
                invokedItems.Contains(listener).ShouldBeTrue();
        }

        [TestMethod]
        public void WeakEventManagerShouldRemoveWeakListenersPropertyChanged()
        {
            const int count = 100;
            const string propertyName = "test";

            var model = new BindingSourceModel();
            var listeners = new List<WeakReference>();
            IWeakEventManager weakEventManager = CreateWeakEventManager();

            for (int i = 0; i < count; i++)
            {
                var listenerMock = new EventListenerMock();
                weakEventManager.Subscribe(model, propertyName, listenerMock);
                listeners.Add(new WeakReference(listenerMock));
                listenerMock.Handle = (o, o1) => { };
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            model.OnPropertyChanged(propertyName + "1");
            model.OnPropertyChanged(propertyName);
            model.OnPropertyChanged(propertyName);

            listeners.All(reference => reference.Target == null).ShouldBeTrue();
        }


        [TestMethod]
        public void WeakEventManagerShouldSubscribeAndUnsubscribeEvent()
        {
            int invokedCount = 0;
            var model = new BindingSourceModel();
            var listenerMock = new EventListenerMock();
            IWeakEventManager weakEventManager = CreateWeakEventManager();
            var disposable = weakEventManager.TrySubscribe(model, BindingSourceModel.EventName, listenerMock);

            listenerMock.Handle = (o, o1) => invokedCount++;

            invokedCount.ShouldEqual(0);
            model.RaiseEvent();
            invokedCount.ShouldEqual(1);

            disposable.Dispose();
            model.RaiseEvent();
            invokedCount.ShouldEqual(1);
        }

        [TestMethod]
        public void WeakEventManagerShouldSubscribeAndUnsubscribeEventSeveralSources()
        {
            const int count = 100;
            var model = new BindingSourceModel();
            var listeners = new List<EventListenerMock>();
            var invokedItems = new List<EventListenerMock>();
            IWeakEventManager weakEventManager = CreateWeakEventManager();

            for (int i = 0; i < count; i++)
            {
                var listenerMock = new EventListenerMock();
                var disposable = weakEventManager.TrySubscribe(model, BindingSourceModel.EventName, listenerMock);
                listeners.Add(listenerMock);
                listenerMock.Handle = (o, o1) =>
                {
                    invokedItems.Add(listenerMock);
                    disposable.Dispose();
                };
            }
            model.RaiseEvent();
            model.RaiseEvent();

            invokedItems.Count.ShouldEqual(count);
            foreach (var listener in listeners)
                invokedItems.Contains(listener).ShouldBeTrue();
        }

        [TestMethod]
        public void WeakEventManagerShouldRemoveWeakListenersEvent()
        {
            const int count = 100;

            var model = new BindingSourceModel();
            var listeners = new List<WeakReference>();
            IWeakEventManager weakEventManager = CreateWeakEventManager();

            for (int i = 0; i < count; i++)
            {
                var listenerMock = new EventListenerMock();
                weakEventManager.TrySubscribe(model, BindingSourceModel.EventName, listenerMock);
                listeners.Add(new WeakReference(listenerMock));
                listenerMock.Handle = (o, o1) => { };
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            model.RaiseEvent();
            model.RaiseEvent();

            listeners.All(reference => reference.Target == null).ShouldBeTrue();
        }

        [TestMethod]
        public void WeakEventManagerShouldReturnNullInvalidEvent()
        {
            var model = new BindingSourceModel();
            IWeakEventManager weakEventManager = CreateWeakEventManager();
            weakEventManager.TrySubscribe(model, BindingSourceModel.InvalidEventName, new EventListenerMock()).ShouldBeNull();
        }

        protected virtual IWeakEventManager CreateWeakEventManager()
        {
            return new WeakEventManager();
        }

        #endregion
    }
}