﻿using System;
using System.IO;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Serialization;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Serialization
{
    public class SerializerTest : ComponentOwnerTestBase<Serializer>
    {
        #region Methods

        [Fact]
        public void CanSerializeShouldReturnFalseNoComponents()
        {
            new Serializer().CanSerialize(typeof(bool)).ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanSerializeShouldBeHandledByComponents(int count)
        {
            var serializer = new Serializer();
            var executeCount = 0;
            var type = typeof(bool);
            var result = false;
            for (var i = 0; i < count; i++)
            {
                var component = new TestSerializerComponent
                {
                    Priority = -i,
                    CanSerialize = (type, context) =>
                    {
                        ++executeCount;
                        type.ShouldEqual(type);
                        context.ShouldEqual(DefaultMetadata);
                        return result;
                    }
                };
                serializer.AddComponent(component);
            }

            serializer.CanSerialize(type, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            result = true;
            serializer.CanSerialize(type, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(1);
        }

        [Fact]
        public void SerializeShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => new Serializer().Serialize(this));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void SerializeShouldBeHandledByComponents(int count)
        {
            var stream = Stream.Null;
            var serializer = new Serializer();
            var executeCount = 0;
            var target = typeof(bool);
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializerComponent
                {
                    TrySerialize = (o, context) =>
                    {
                        ++executeCount;
                        o.ShouldEqual(target);
                        context.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return stream;
                        return null;
                    },
                    Priority = -i
                };
                serializer.AddComponent(component);
            }

            serializer.Serialize(target, DefaultMetadata).ShouldEqual(stream);
            executeCount.ShouldEqual(count);
        }

        [Fact]
        public void DeserializeShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => new Serializer().Deserialize(Stream.Null));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DeserializeShouldBeHandledByComponents(int count)
        {
            var stream = Stream.Null;
            var serializer = new Serializer();
            var executeCount = 0;
            var target = typeof(bool);
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializerComponent
                {
                    TryDeserialize = (o, context) =>
                    {
                        ++executeCount;
                        o.ShouldEqual(stream);
                        context.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return target;
                        return null;
                    },
                    Priority = -i
                };
                serializer.AddComponent(component);
            }

            serializer.Deserialize(stream, DefaultMetadata).ShouldEqual(target);
            executeCount.ShouldEqual(count);
        }

        protected override Serializer GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new Serializer(collectionProvider);
        }

        #endregion
    }
}