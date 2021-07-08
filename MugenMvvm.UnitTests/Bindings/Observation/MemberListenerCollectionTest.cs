using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation
{
    [Collection(SharedContext)]
    public class MemberListenerCollectionTest : UnitTestBase
    {
        public MemberListenerCollectionTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(500)]
        public void RaiseShouldHandleDifferentNames(int count)
        {
            var listeners = new TestWeakEventListener[count];
            for (var i = 0; i < listeners.Length; i++)
            {
                var index = i;
                listeners[index] = new TestWeakEventListener
                {
                    IsAlive = true,
                    IsWeak = i % 2 == 0
                };
            }

            var listener = new MemberListenerCollection();
            for (var i = 0; i < count; i++)
                listener.Add(listeners[i], $"{i}");

            for (var i = 0; i < count; i++)
            {
                listener.RaisePropertyChanged(this, new PropertyChangedEventArgs($"{i}"));
                ValidateInvokeCount(listeners, 1, false, 0, i + 1);
                ValidateInvokeCount(listeners, 0, false, i + 1);
            }

            ValidateInvokeCount(listeners, 1);
            listener.RaisePropertyChanged(this, Default.EmptyPropertyChangedArgs);
            ValidateInvokeCount(listeners, 1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(500)]
        public void RaiseShouldHandleIndexerNames(int count)
        {
            var listeners = new TestWeakEventListener[count];
            for (var i = 0; i < listeners.Length; i++)
            {
                var index = i;
                listeners[index] = new TestWeakEventListener
                {
                    IsAlive = true,
                    IsWeak = i % 2 == 0
                };
            }

            var listener = new MemberListenerCollection();
            for (var i = 0; i < count; i++)
                listener.Add(listeners[i], $"[{i}]");

            for (var i = 0; i < count; i++)
            {
                listener.RaisePropertyChanged(this, new PropertyChangedEventArgs($"[{i}]"));
                ValidateInvokeCount(listeners, 1, false, 0, i + 1);
                ValidateInvokeCount(listeners, 0, false, i + 1);
            }

            ValidateInvokeCount(listeners, 1);
            for (var i = 0; i < count; i++)
            {
                listener.RaisePropertyChanged(this, new PropertyChangedEventArgs($"Item[{i}]"));
                ValidateInvokeCount(listeners, 1, false, 0, i + 1);
                ValidateInvokeCount(listeners, 0, false, i + 1);
            }

            ValidateInvokeCount(listeners, 1);

            listener.RaisePropertyChanged(this, Default.IndexerPropertyChangedArgs);
            ValidateInvokeCount(listeners, 1);

            listener.RaisePropertyChanged(this, Default.EmptyPropertyChangedArgs);
            ValidateInvokeCount(listeners, 1);
        }

        [Fact]
        public void ShouldInvokeVirtualMethods()
        {
            var l1 = new TestWeakEventListener();
            var l2 = new TestWeakEventListener();
            var l3 = new TestWeakEventListener();

            var collection = new TestPropertyChangedListenerCollection();
            var token = collection.Add(l1, "");
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            collection.Add(l2, "");
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            collection.Add(l3, "");
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            l2.TryHandleDefault = false;
            l3.TryHandleDefault = false;
            collection.Raise(this, this, "", DefaultMetadata);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            token.Dispose();
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(1);

            collection.AddedCount = 0;
            collection.RemovedCount = 0;
            token = collection.Add(l1, "");
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);

            token.Dispose();
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(1);

            collection.AddedCount = 0;
            collection.RemovedCount = 0;
            collection.Add(l1, "");
            collection.Add(l2, "");
            collection.Add(l3, "");
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);
            l1.TryHandleDefault = false;
            l2.TryHandleDefault = false;
            l3.TryHandleDefault = false;
            collection.Raise(this, this, "", DefaultMetadata);
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(1);

            collection.AddedCount = 0;
            collection.RemovedCount = 0;
            collection.Add(l1, "");
            collection.Add(l2, "");
            collection.Add(l3, "");
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(0);
            collection.Clear();
            collection.AddedCount.ShouldEqual(1);
            collection.RemovedCount.ShouldEqual(1);
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
                        o1.ShouldEqual(args);
                        return listeners[index].IsAlive;
                    }
                };
            }

            var collection = new MemberListenerCollection();
            for (var i = 0; i < count; i++)
            {
                collection.Add(listeners[i], args.PropertyName!);
                collection.RaisePropertyChanged(sender, args);
                ValidateInvokeCount(listeners, 1, true, 0, i + 1);
                collection.Count.ShouldEqual(i + 1);
            }

            var removeCount = Math.Min(count, 100);
            for (var i = 0; i < removeCount; i++)
                listeners[i].IsAlive = false;

            collection.RaisePropertyChanged(sender, args);
            collection.Count.ShouldEqual(listeners.Length - removeCount);
            ValidateInvokeCount(listeners, 1);

            collection.RaisePropertyChanged(sender, args);
            collection.Count.ShouldEqual(listeners.Length - removeCount);
            ValidateInvokeCount(listeners, 1, true, removeCount);

            var tokens = new List<ActionToken>();
            for (var i = 0; i < removeCount; i++)
            {
                listeners[i].IsAlive = true;
                tokens.Add(collection.Add(listeners[i], args.PropertyName!));
            }

            collection.RaisePropertyChanged(sender, args);
            collection.Count.ShouldEqual(listeners.Length);
            ValidateInvokeCount(listeners, 1);


            for (var index = 0; index < removeCount; index++)
            {
                tokens[index].Dispose();
                collection.RaisePropertyChanged(sender, args);
                ValidateInvokeCount(listeners, 1, true, index + 1);
            }

            for (var i = 0; i < removeCount; i++)
                collection.Add(listeners[i], args.PropertyName);

            collection.RaisePropertyChanged(sender, args);
            ValidateInvokeCount(listeners, 1);
            collection.Count.ShouldEqual(listeners.Length);

            for (var i = 0; i < count; i++)
                listeners[i].IsAlive = false;

            collection.RaisePropertyChanged(sender, args);
            ValidateInvokeCount(listeners, 1);
            collection.Count.ShouldEqual(0);

            collection.RaisePropertyChanged(sender, args);
            ValidateInvokeCount(listeners, 0);
            collection.Count.ShouldEqual(0);
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

        private sealed class TestPropertyChangedListenerCollection : MemberListenerCollection
        {
            public int RemovedCount { get; set; }

            public int AddedCount { get; set; }

            protected override void OnListenersAdded() => ++AddedCount;

            protected override void OnListenersRemoved() => ++RemovedCount;
        }
    }
}