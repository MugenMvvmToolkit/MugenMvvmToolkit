using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observation
{
    public class EventListenerCollectionTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(500)]
        public void ShouldManagerSubscribers(int count)
        {
            var sender = this;
            var msg = new object();
            var listeners = new TestWeakEventListener[count];

            for (var i = 0; i < listeners.Length; i++)
            {
                var index = i;
                listeners[index] = new TestWeakEventListener
                {
                    IsAlive = true,
                    IsWeak = i % 2 == 0,
                    TryHandle = (o, o1, m) =>
                    {
                        o.ShouldEqual(sender);
                        o1.ShouldEqual(msg);
                        m.ShouldEqual(DefaultMetadata);
                        return listeners[index].IsAlive;
                    }
                };
            }

            var collection = new EventListenerCollection();
            collection.HasListeners.ShouldBeFalse();
            for (var i = 0; i < count; i++)
            {
                collection.Add(listeners[i]);
                collection.Raise(sender, msg, DefaultMetadata);
                ValidateInvokeCount(listeners, 1, true, 0, i + 1);
                collection.HasListeners.ShouldBeTrue();
            }

            var removeCount = Math.Min(count, 100);
            for (var i = 0; i < removeCount; i++)
                listeners[i].IsAlive = false;

            collection.Raise(sender, msg, DefaultMetadata);
            collection.HasListeners.ShouldEqual(count != 1);
            ValidateInvokeCount(listeners, 1);

            collection.Raise(sender, msg, DefaultMetadata);
            collection.HasListeners.ShouldEqual(count != 1);
            ValidateInvokeCount(listeners, 1, true, removeCount);

            var tokens = new List<ActionToken>();
            for (var i = 0; i < removeCount; i++)
            {
                listeners[i].IsAlive = true;
                tokens.Add(collection.Add(listeners[i]));
            }

            collection.Raise(sender, msg, DefaultMetadata);
            collection.HasListeners.ShouldBeTrue();
            ValidateInvokeCount(listeners, 1);

            for (var index = 0; index < removeCount; index++)
            {
                tokens[index].Dispose();
                collection.Raise(sender, msg, DefaultMetadata);
                collection.HasListeners.ShouldEqual(count != 1);
                ValidateInvokeCount(listeners, 1, true, index + 1);
            }

            for (var i = 0; i < removeCount; i++)
                collection.Add(listeners[i]);

            collection.Raise(sender, msg, DefaultMetadata);
            collection.HasListeners.ShouldBeTrue();
            ValidateInvokeCount(listeners, 1);

            for (var index = 0; index < removeCount; index++)
            {
                collection.Remove(listeners[index]);
                collection.Raise(sender, msg, DefaultMetadata);
                ValidateInvokeCount(listeners, 1, true, index + 1);
            }

            for (var i = 0; i < removeCount; i++)
                collection.Add(listeners[i]);

            collection.Raise(sender, msg, DefaultMetadata);
            ValidateInvokeCount(listeners, 1);

            collection.Clear();
            collection.HasListeners.ShouldBeFalse();
            collection.Raise(sender, msg, DefaultMetadata);
            ValidateInvokeCount(listeners, 0);
        }

        [Fact]
        public void ShouldInvokeVirtualMethods()
        {
            var l1 = new TestWeakEventListener();
            var l2 = new TestWeakEventListener();
            var l3 = new TestWeakEventListener();

            var collection = new TestEventListenerCollection();
            collection.Add(l1);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            collection.Add(l2);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            collection.Add(l3);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            l2.TryHandleDefault = false;
            l3.TryHandleDefault = false;
            collection.Raise(this, this, DefaultMetadata);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            collection.Remove(l1);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(1);

            collection.AddedCount = 0;
            collection.RemovedCount = 0;
            collection.Add(l1);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            collection.Remove(l1);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(1);

            collection.AddedCount = 0;
            collection.RemovedCount = 0;
            var token = collection.Add(l1);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            token.Dispose();
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(1);

            collection.AddedCount = 0;
            collection.RemovedCount = 0;
            collection.Add(l1);
            collection.Add(l2);
            collection.Add(l3);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);
            l1.TryHandleDefault = false;
            l2.TryHandleDefault = false;
            l3.TryHandleDefault = false;
            collection.Raise(this, this, DefaultMetadata);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(1);
        }

        private static void ValidateInvokeCount(TestWeakEventListener[] listeners, int count, bool clear = true, int? start = null, int? end = null)
        {
            for (var i = start.GetValueOrDefault(); i < end.GetValueOrDefault(listeners.Length); i++)
            {
                listeners[i].InvokeCount.ShouldEqual(count);
                if (clear)
                    listeners[i].InvokeCount = 0;
            }
        }

        #endregion

        #region Nested types

        private sealed class TestEventListenerCollection : EventListenerCollection
        {
            #region Properties

            public int RemovedCount { get; set; }

            public int AddedCount { get; set; }

            #endregion

            #region Methods

            protected override void OnListenersAdded()
            {
                ++AddedCount;
            }

            protected override void OnListenersRemoved()
            {
                ++RemovedCount;
            }

            #endregion
        }

        #endregion
    }
}