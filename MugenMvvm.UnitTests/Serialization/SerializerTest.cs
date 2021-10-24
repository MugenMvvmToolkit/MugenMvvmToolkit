using System;
using System.IO;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Serialization;
using MugenMvvm.Tests.Serialization;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Serialization
{
    public class SerializerTest : ComponentOwnerTestBase<Serializer>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DeserializeShouldBeHandledByComponents(int count)
        {
            var format = new DeserializationFormat<string, Stream?>("Test");
            var request = "test";
            var ctx = new SerializationContext<string, Stream>(format, request);
            var result = Stream.Null;
            Serializer.AddComponent(new TestSerializationContextProvider
            {
                TryGetSerializationContext = (s, f, r, arg4) =>
                {
                    s.ShouldEqual(Serializer);
                    f.ShouldEqual(format);
                    r.ShouldEqual(request);
                    arg4.ShouldEqual(Metadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializationManagerComponent
                {
                    TryDeserialize = (s, t, context) =>
                    {
                        ++executeCount;
                        s.ShouldEqual(Serializer);
                        t.ShouldEqual(request);
                        context.ShouldEqual(ctx);
                        if (isLast)
                            return result;
                        return null;
                    },
                    Priority = -i
                };
                Serializer.AddComponent(component);
            }

            Serializer.Deserialize(format, request, null, Metadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
        }

        [Fact]
        public void DeserializeShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => Serializer.Deserialize(DeserializationFormat.JsonBytes, null!));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsSupportedShouldBeHandledByComponents(int count)
        {
            var format = new SerializationFormat<string, Stream?>("Test");
            var request = "r";
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializationManagerComponent
                {
                    IsSupported = (s, t, r, context) =>
                    {
                        ++executeCount;
                        s.ShouldEqual(Serializer);
                        t.ShouldEqual(format);
                        r.ShouldEqual(request);
                        context.ShouldEqual(Metadata);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                };
                Serializer.AddComponent(component);
            }

            Serializer.IsSupported(format, request, Metadata).ShouldEqual(true);
            executeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void SerializeShouldBeHandledByComponents(int count)
        {
            var format = new SerializationFormat<string, Stream?>("Test");
            var request = "test";
            var ctx = new SerializationContext<string, Stream>(format, request);
            var result = Stream.Null;
            Serializer.AddComponent(new TestSerializationContextProvider
            {
                TryGetSerializationContext = (s, f, r, arg4) =>
                {
                    s.ShouldEqual(Serializer);
                    f.ShouldEqual(format);
                    r.ShouldEqual(request);
                    arg4.ShouldEqual(Metadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializationManagerComponent
                {
                    TrySerialize = (s, t, context) =>
                    {
                        ++executeCount;
                        s.ShouldEqual(Serializer);
                        t.ShouldEqual(request);
                        context.ShouldEqual(ctx);
                        if (isLast)
                            return result;
                        return null;
                    },
                    Priority = -i
                };
                Serializer.AddComponent(component);
            }

            Serializer.Serialize(format, request, null, Metadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
        }

        [Fact]
        public void SerializeShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => Serializer.Serialize(SerializationFormat.JsonBytes!, null!));

        protected override ISerializer GetSerializer() => GetComponentOwner(ComponentCollectionManager);

        protected override Serializer GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}