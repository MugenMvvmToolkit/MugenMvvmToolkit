using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Observers;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
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
            var listeners = new TestEventListener[count];

            for (var i = 0; i < listeners.Length; i++)
            {
                var index = i;
                listeners[index] = new TestEventListener
                {
                    IsAlive = true,
                    IsWeak = i % 2 == 0,
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
                collection.Add(listeners[i]);
                collection.Raise(sender, msg);
                ValidateInvokeCount(listeners, 1, true, 0, i + 1);
            }

            var removeCount = Math.Min(count, 100);
            for (var i = 0; i < removeCount; i++)
                listeners[i].IsAlive = false;

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1);

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1, true, removeCount);

            var tokens = new List<ActionToken>();
            for (var i = 0; i < removeCount; i++)
            {
                listeners[i].IsAlive = true;
                tokens.Add(collection.Add(listeners[i]));
            }

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1);

            for (var index = 0; index < removeCount; index++)
            {
                tokens[index].Dispose();
                collection.Raise(sender, msg);
                ValidateInvokeCount(listeners, 1, true, index + 1);
            }

            for (var i = 0; i < removeCount; i++)
                collection.Add(listeners[i]);

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1);


            for (var index = 0; index < removeCount; index++)
            {
                collection.Remove(listeners[index]);
                collection.Raise(sender, msg);
                ValidateInvokeCount(listeners, 1, true, index + 1);
            }

            for (var i = 0; i < removeCount; i++)
                collection.Add(listeners[i]);

            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 1);

            collection.Clear();
            collection.Raise(sender, msg);
            ValidateInvokeCount(listeners, 0);
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