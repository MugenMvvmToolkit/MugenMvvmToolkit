using System;
using System.IO;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Serialization;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Serialization.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Serialization
{
    public class SerializerTest : ComponentOwnerTestBase<Serializer>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsSupportedShouldBeHandledByComponents(int count)
        {
            var format = new SerializationFormat<string, Stream?>(1, "Test");
            var request = "r";
            var serializer = new Serializer();
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializationManagerComponent(serializer)
                {
                    IsSupported = (t, r, context) =>
                    {
                        ++executeCount;
                        t.ShouldEqual(format);
                        r.ShouldEqual(request);
                        context.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                };
                serializer.AddComponent(component);
            }

            serializer.IsSupported(format, request, DefaultMetadata).ShouldEqual(true);
            executeCount.ShouldEqual(count);
        }

        [Fact]
        public void SerializeShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => new Serializer().Serialize(SerializationFormat.JsonBytes!, null!));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void SerializeShouldBeHandledByComponents(int count)
        {
            var format = new SerializationFormat<string, Stream?>(1, "Test");
            var request = "test";
            var ctx = new SerializationContext<string, Stream>(format, request);
            var result = Stream.Null;
            var serializer = new Serializer();
            serializer.AddComponent(new TestSerializationContextProvider(serializer)
            {
                TryGetSerializationContext = (f, r, arg4) =>
                {
                    f.ShouldEqual(format);
                    r.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializationManagerComponent(serializer)
                {
                    TrySerialize = (t, context) =>
                    {
                        ++executeCount;
                        t.ShouldEqual(request);
                        context.ShouldEqual(ctx);
                        if (isLast)
                            return result;
                        return null;
                    },
                    Priority = -i
                };
                serializer.AddComponent(component);
            }

            serializer.Serialize(format, request, null, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
        }

        [Fact]
        public void DeserializeShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => new Serializer().Deserialize(DeserializationFormat.JsonBytes, null!));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DeserializeShouldBeHandledByComponents(int count)
        {
            var format = new DeserializationFormat<string, Stream?>(1, "Test");
            var request = "test";
            var ctx = new SerializationContext<string, Stream>(format, request);
            var result = Stream.Null;
            var serializer = new Serializer();
            serializer.AddComponent(new TestSerializationContextProvider(serializer)
            {
                TryGetSerializationContext = (f, r, arg4) =>
                {
                    f.ShouldEqual(format);
                    r.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializationManagerComponent(serializer)
                {
                    TryDeserialize = (t, context) =>
                    {
                        ++executeCount;
                        t.ShouldEqual(request);
                        context.ShouldEqual(ctx);
                        if (isLast)
                            return result;
                        return null;
                    },
                    Priority = -i
                };
                serializer.AddComponent(component);
            }

            serializer.Deserialize(format, request, null, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
        }

        protected override Serializer GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new(collectionProvider);

        #endregion
    }
}