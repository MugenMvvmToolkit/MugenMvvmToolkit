using System.IO;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Serialization;
using MugenMvvm.Serialization.Components;
using MugenMvvm.UnitTests.Serialization.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Serialization.Components
{
    public class SerializationManagerTest : UnitTestBase
    {
        private readonly SerializationFormat<string, Stream?> _serializationFormat;
        private readonly DeserializationFormat<string, Stream?> _deserializationFormat;
        private readonly Serializer _serializer;

        public SerializationManagerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _serializationFormat = new SerializationFormat<string, Stream?>(1, "Test");
            _deserializationFormat = new DeserializationFormat<string, Stream?>(1, "Test");
            _serializer = new Serializer(ComponentCollectionManager);
            _serializer.AddComponent(new SerializationManager());
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
                var component = new TestSerializerComponent<string, Stream?>(_serializer)
                {
                    IsSupported = (t, r, context) =>
                    {
                        ++executeCount;
                        t.ShouldEqual(_serializationFormat);
                        r.ShouldEqual(request);
                        context.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                };
                _serializer.AddComponent(component);
            }

            _serializer.IsSupported(_serializationFormat, request, DefaultMetadata).ShouldEqual(true);
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
                var component = new TestDeserializerComponent<string, Stream?>(_serializer)
                {
                    IsSupported = (t, r, context) =>
                    {
                        ++executeCount;
                        t.ShouldEqual(_deserializationFormat);
                        r.ShouldEqual(request);
                        context.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                };
                _serializer.AddComponent(component);
            }

            _serializer.IsSupported(_deserializationFormat, request, DefaultMetadata).ShouldEqual(true);
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
            _serializer.AddComponent(new TestSerializationContextProvider(_serializer)
            {
                TryGetSerializationContext = (f, r, arg4) =>
                {
                    f.ShouldEqual(_serializationFormat);
                    r.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializerComponent<string, Stream?>(_serializer)
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
                _serializer.AddComponent(component);
            }

            _serializer.Serialize(_serializationFormat, request, null, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DeserializeShouldBeHandledByComponents(int count)
        {
            var request = "test";
            var ctx = new SerializationContext<string, Stream>(_deserializationFormat, request);
            var result = Stream.Null;
            _serializer.AddComponent(new TestSerializationContextProvider(_serializer)
            {
                TryGetSerializationContext = (f, r, arg4) =>
                {
                    f.ShouldEqual(_deserializationFormat);
                    r.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return ctx;
                }
            });
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                _serializer.AddComponent(new TestDeserializerComponent<string, Stream?>(_serializer)
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
                });
            }

            _serializer.Deserialize(_deserializationFormat, request, null, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
        }
    }
}