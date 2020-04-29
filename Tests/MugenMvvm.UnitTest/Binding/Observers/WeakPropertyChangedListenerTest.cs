using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Binding.Observers;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class WeakPropertyChangedListenerTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(500)]
        public void RaiseShouldHandleDifferentNames(int count)
        {
            var listeners = new TestEventListener[count];
            for (var i = 0; i < listeners.Length; i++)
            {
                var index = i;
                listeners[index] = new TestEventListener
                {
                    IsAlive = true,
                    IsWeak = i % 2 == 0
                };
            }

            var listener = new WeakPropertyChangedListener();
            for (var i = 0; i < count; i++)
                listener.Add(listeners[i], $"{i}");

            for (var i = 0; i < count; i++)
            {
                listener.Raise(this, new PropertyChangedEventArgs($"{i}"));
                ValidateInvokeCount(listeners, 1, false, 0, i + 1);
                ValidateInvokeCount(listeners, 0, false, i + 1);
            }

            ValidateInvokeCount(listeners, 1);
            listener.Raise(this, Default.EmptyPropertyChangedArgs);
            ValidateInvokeCount(listeners, 1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(500)]
        public void RaiseShouldHandleIndexerNames(int count)
        {
            var listeners = new TestEventListener[count];
            for (var i = 0; i < listeners.Length; i++)
            {
                var index = i;
                listeners[index] = new TestEventListener
                {
                    IsAlive = true,
                    IsWeak = i % 2 == 0
                };
            }

            var listener = new WeakPropertyChangedListener();
            for (var i = 0; i < count; i++)
                listener.Add(listeners[i], $"[{i}]");

            for (var i = 0; i < count; i++)
            {
                listener.Raise(this, new PropertyChangedEventArgs($"[{i}]"));
                ValidateInvokeCount(listeners, 1, false, 0, i + 1);
                ValidateInvokeCount(listeners, 0, false, i + 1);
            }

            ValidateInvokeCount(listeners, 1);
            for (var i = 0; i < count; i++)
            {
                listener.Raise(this, new PropertyChangedEventArgs($"Item[{i}]"));
                ValidateInvokeCount(listeners, 1, false, 0, i + 1);
                ValidateInvokeCount(listeners, 0, false, i + 1);
            }

            ValidateInvokeCount(listeners, 1);

            listener.Raise(this, Default.IndexerPropertyChangedArgs);
            ValidateInvokeCount(listeners, 1);

            listener.Raise(this, Default.EmptyPropertyChangedArgs);
            ValidateInvokeCount(listeners, 1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(500)]
        public void ShouldManagerSubscribers(int count)
        {
            var sender = this;
            var args = new PropertyChangedEventArgs("Test");
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
                        o1.ShouldEqual(args);
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
                        o1.ShouldEqual(args);
                        return listeners[index].IsAlive;
                    }
                };
            }

            var listener = new WeakPropertyChangedListener();
            for (var i = 0; i < count; i++)
            {
                listener.Add(weakListeners[i], args.PropertyName);
                listener.Add(listeners[i], args.PropertyName);

                listener.Raise(sender, args);
                ValidateInvokeCount(listeners, 1, true, 0, i + 1);
                ValidateInvokeCount(weakListeners, 1, true, 0, i + 1);
            }

            var removeCount = Math.Min(count, 100);
            for (var i = 0; i < removeCount; i++)
            {
                weakListeners[i].IsAlive = false;
                listeners[i].IsAlive = false;
            }

            listener.Raise(sender, args);
            ValidateInvokeCount(listeners, 1);
            ValidateInvokeCount(weakListeners, 1);

            listener.Raise(sender, args);
            ValidateInvokeCount(listeners, 1, true, removeCount);
            ValidateInvokeCount(weakListeners, 1, true, removeCount);

            var weakTokens = new List<ActionToken>();
            var tokens = new List<ActionToken>();
            for (var i = 0; i < removeCount; i++)
            {
                weakListeners[i].IsAlive = true;
                listeners[i].IsAlive = true;
                weakTokens.Add(listener.Add(weakListeners[i], args.PropertyName));
                tokens.Add(listener.Add(listeners[i], args.PropertyName));
            }

            listener.Raise(sender, args);
            ValidateInvokeCount(listeners, 1);
            ValidateInvokeCount(weakListeners, 1);


            for (var index = 0; index < removeCount; index++)
            {
                tokens[index].Dispose();
                weakTokens[index].Dispose();
                listener.Raise(sender, args);
                ValidateInvokeCount(listeners, 1, true, index + 1);
                ValidateInvokeCount(weakListeners, 1, true, index + 1);
            }

            for (var i = 0; i < removeCount; i++)
            {
                listener.Add(weakListeners[i], args.PropertyName);
                listener.Add(listeners[i], args.PropertyName);
            }

            listener.Raise(sender, args);
            ValidateInvokeCount(listeners, 1);
            ValidateInvokeCount(weakListeners, 1);


            for (var i = 0; i < count; i++)
            {
                weakListeners[i].IsAlive = false;
                listeners[i].IsAlive = false;
            }

            listener.Raise(sender, args);
            ValidateInvokeCount(listeners, 1);
            ValidateInvokeCount(weakListeners, 1);

            listener.Raise(sender, args);
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