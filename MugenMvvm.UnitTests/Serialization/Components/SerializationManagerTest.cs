using System.IO;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Serialization;
using MugenMvvm.Serialization.Components;
using MugenMvvm.Tests.Serialization;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Serialization.Components
{
    public class SerializationManagerTest : UnitTestBase
    {
        private readonly SerializationFormat<string, Stream?> _serializationFormat;
        private readonly DeserializationFormat<string, Stream?> _deserializationFormat;

        public SerializationManagerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _serializationFormat = new SerializationFormat<string, Stream?>("Test");
            _deserializationFormat = new DeserializationFormat<string, Stream?>("Test");
            Serializer.AddComponent(new SerializationManager());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DeserializeShouldBeHandledByComponents(int count)
        {
            var request = "test";
            var ctx = new SerializationContext<string, Stream>(_deserializationFormat, request);
            var result = Stream.Null;
            Serializer.AddComponent(new TestSerializationContextProvider
            {
                TryGetSerializationContext = (s, f, r, arg4) =>
                {
                    s.ShouldEqual(Serializer);
                    f.ShouldEqual(_deserializationFormat);
                    r.ShouldEqual(request);
                    arg4.ShouldEqual(Metadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                Serializer.AddComponent(new TestDeserializerComponent<string, Stream?>
                {
                    TryDeserialize = (s, f, t, _, context) =>
                    {
                        ++executeCount;
                        s.ShouldEqual(Serializer);
                        f.ShouldEqual(_deserializationFormat);
                        t.ShouldEqual(request);
                        context.ShouldEqual(ctx);
                        if (isLast)
                            return result;
                        return null;
                    },
                    Priority = -i
                });
            }

            Serializer.Deserialize(_deserializationFormat, request, null, Metadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsSupportedShouldBeHandledByComponentsDeserializer(int count)
        {
            var request = "r";
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestDeserializerComponent<string, Stream?>
                {
                    IsSupported = (s, t, r, context) =>
                    {
                        ++executeCount;
                        s.ShouldEqual(Serializer);
                        t.ShouldEqual(_deserializationFormat);
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

            Serializer.IsSupported(_deserializationFormat, request, Metadata).ShouldEqual(true);
            executeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsSupportedShouldBeHandledByComponentsSerializer(int count)
        {
            var request = "r";
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializerComponent<string, Stream?>
                {
                    IsSupported = (s, t, r, context) =>
                    {
                        ++executeCount;
                        s.ShouldEqual(Serializer);
                        t.ShouldEqual(_serializationFormat);
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

            Serializer.IsSupported(_serializationFormat, request, Metadata).ShouldEqual(true);
            executeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void SerializeShouldBeHandledByComponents(int count)
        {
            var request = "test";
            var ctx = new SerializationContext<string, Stream>(_serializationFormat, request);
            var result = Stream.Null;
            Serializer.AddComponent(new TestSerializationContextProvider
            {
                TryGetSerializationContext = (s, f, r, arg4) =>
                {
                    s.ShouldEqual(Serializer);
                    f.ShouldEqual(_serializationFormat);
                    r.ShouldEqual(request);
                    arg4.ShouldEqual(Metadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializerComponent<string, Stream?>
                {
                    TrySerialize = (s, f, t, _, context) =>
                    {
                        ++executeCount;
                        s.ShouldEqual(Serializer);
                        f.ShouldEqual(_serializationFormat);
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

            Serializer.Serialize(_serializationFormat, request, null, Metadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
        }

        protected override ISerializer GetSerializer() => new Serializer(ComponentCollectionManager);
    }
}