using System;
using System.IO;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Serialization;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Serialization.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Serialization
{
    public class SerializerTest : ComponentOwnerTestBase<Serializer>
    {
        #region Methods

        [Fact]
        public void SerializeShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => new Serializer().Serialize(new MemoryStream(), this));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void SerializeShouldBeHandledByComponents(int count)
        {
            var ctx = new SerializationContext(Stream.Null, false);
            var stream = Stream.Null;
            var serializer = new Serializer();
            serializer.AddComponent(new TestSerializationContextProvider(serializer)
            {
                TryGetSerializationContext = (s, o, arg4) =>
                {
                    o.ShouldEqual(this);
                    s.ShouldEqual(stream);
                    arg4.ShouldEqual(DefaultMetadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializerComponent(serializer)
                {
                    TrySerialize = (t, context) =>
                    {
                        ++executeCount;
                        t.ShouldEqual(this);
                        context.ShouldEqual(ctx);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                };
                serializer.AddComponent(component);
            }

            serializer.Serialize(stream, this, DefaultMetadata);
            executeCount.ShouldEqual(count);
        }

        [Fact]
        public void DeserializeShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => new Serializer().Deserialize(new MemoryStream()));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DeserializeShouldBeHandledByComponents(int count)
        {
            var ctx = new SerializationContext(Stream.Null, false);
            var stream = Stream.Null;
            var serializer = new Serializer();
            serializer.AddComponent(new TestSerializationContextProvider(serializer)
            {
                TryGetDeserializationContext = (s, arg4) =>
                {
                    s.ShouldEqual(stream);
                    arg4.ShouldEqual(DefaultMetadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            var target = typeof(bool);
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializerComponent(serializer)
                {
                    TryDeserialize = (context) =>
                    {
                        ++executeCount;
                        context.ShouldEqual(ctx);
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

        protected override Serializer GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new Serializer(collectionProvider);

        #endregion
    }
}