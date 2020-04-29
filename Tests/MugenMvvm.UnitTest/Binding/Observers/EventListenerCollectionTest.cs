using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Observers;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
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
            var msg = DefaultMetadata;
            var weakListeners = new TestEventListener[count];
            var listeners = new TestEventListener[count];

            for (var i = 0; i < weakListeners.Length; i++)
            {
                var index = i;
                weakListeners[index] = new TestEventListener
                {
                    IsAlive = true,
                    IsWeak = true,
                    TryHandle = (o, o1) =>
                    {
                        o.ShouldEqual(sender);
                        o1.ShouldEqual(DefaultMetadata);
                        return weakListeners[index].IsAlive;
                    }
                };
            }

            for (var i = 0; i < listeners.Length; i++)
            {
                var index = i;
                listeners[index] = new TestEventListener
                {
                    IsAlive = true,
                    IsWeak = false,
                    TryHandle = (o, o1) =>
                    {
                        o.ShouldEqual(sender);
                        o1.ShouldEqual(DefaultMetadata);
                        return listeners[index].IsAlive;
                    }
                };
            }

            var collection = new EventListenerCollection();
            for (var i = 0; i < count; i++)
            {
                collection.Add(weakListeners[i]);
                collection.Add(listeners[i]);

                collection.Raise(sender, msg);
                ValidateInvokeCount(listeners, 1, true, 0, i + 1);
                ValidateInvokeCount(weakListeners, 1, true, 0, i + 1);
            }

            var removeCount = Math.Min(count, 100);
            for (var i = 0; i < removeCount; i++)
            {
                weakListeners[i].IsAlive = false;
                listeners[i].IsAlive = false;
            }

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1);
            ValidateInvokeCount(weakListeners, 1);

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1, true, removeCount);
            ValidateInvokeCount(weakListeners, 1, true, removeCount);

            var weakTokens = new List<ActionToken>();
            var tokens = new List<ActionToken>();
            for (var i = 0; i < removeCount; i++)
            {
                weakListeners[i].IsAlive = true;
                listeners[i].IsAlive = true;
                weakTokens.Add(collection.Add(weakListeners[i]));
                tokens.Add(collection.Add(listeners[i]));
            }

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1);
            ValidateInvokeCount(weakListeners, 1);


            for (var index = 0; index < removeCount; index++)
            {
                tokens[index].Dispose();
                weakTokens[index].Dispose();
                collection.Raise(sender, msg);
                ValidateInvokeCount(listeners, 1, true, index + 1);
                ValidateInvokeCount(weakListeners, 1, true, index + 1);
            }

            for (var i = 0; i < removeCount; i++)
            {
                collection.Add(weakListeners[i]);
                collection.Add(listeners[i]);
            }

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1);
            ValidateInvokeCount(weakListeners, 1);


            for (var index = 0; index < removeCount; index++)
            {
                collection.Remove(listeners[index]);
                collection.Remove(weakListeners[index]);
                collection.Raise(sender, msg);
                ValidateInvokeCount(listeners, 1, true, index + 1);
                ValidateInvokeCount(weakListeners, 1, true, index + 1);
            }

            for (var i = 0; i < removeCount; i++)
            {
                collection.Add(weakListeners[i]);
                collection.Add(listeners[i]);
            }

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1);
            ValidateInvokeCount(weakListeners, 1);

            collection.Clear();
            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 0);
            ValidateInvokeCount(weakListeners, 0);
        }

        private static void ValidateInvokeCount(TestEventListener[] listeners, int count, bool clear = true, int? start = null, int? end = null)
        {
            for (var i = start.GetValueOrDefault(); i < end.GetValueOrDefault(listeners.Length); i++)
            {
                listeners[i].InvokeCount.ShouldEqual(count);
                if (clear)
                    listeners[i].InvokeCount = 0;
            }
        }

        #endregion
    }
}