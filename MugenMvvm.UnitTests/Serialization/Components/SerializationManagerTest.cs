using System.IO;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Serialization;
using MugenMvvm.Serialization.Components;
using MugenMvvm.UnitTests.Serialization.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Serialization.Components
{
    public class SerializationManagerTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsSupportedShouldBeHandledByComponentsSerializer(int count)
        {
            var format = new SerializationFormat<string, Stream?>(1, "Test");
            var request = "r";
            var serializer = new Serializer();
            serializer.AddComponent(new SerializationManager());
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestSerializerComponent<string, Stream?>(serializer)
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

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsSupportedShouldBeHandledByComponentsDeserializer(int count)
        {
            var format = new DeserializationFormat<string, Stream?>(1, "Test");
            var request = "r";
            var serializer = new Serializer();
            serializer.AddComponent(new SerializationManager());
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestDeserializerComponent<string, Stream?>(serializer)
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
            serializer.AddComponent(new SerializationManager());
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
                var component = new TestSerializerComponent<string, Stream?>(serializer)
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
            serializer.AddComponent(new SerializationManager());
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
                var component = new TestDeserializerComponent<string, Stream?>(serializer)
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

        #endregion
    }
}