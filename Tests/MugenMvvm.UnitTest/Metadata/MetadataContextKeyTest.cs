using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.Serialization;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public class MetadataContextKeyTest : UnitTestBase
    {
        #region Properties

        public static IMetadataContextKey<int>? ContextKeyField;

        public static IMetadataContextKey<int>? ContextKeyProperty { get; set; }

        #endregion

        #region Methods

        [Fact]
        public void FromKeyShouldCreateMetadataKeyFromString()
        {
            var key = MetadataContextKey.FromKey<int>("test");
            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            (key is ISerializableMetadataContextKey).ShouldBeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromMemberShouldCreateMetadataKeyFromField(bool serializable)
        {
            var key = MetadataContextKey.FromMember<int>(GetType(), nameof(ContextKeyField), serializable);
            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            var contextKey = (key as ISerializableMetadataContextKey)!;
            if (!serializable)
            {
                contextKey.ShouldBeNull();
                return;
            }

            var serializationContext = new SerializationContext(DefaultMetadata);
            contextKey.CanSerialize(1, serializationContext).ShouldBeTrue();
            contextKey.Serialize(1, serializationContext).ShouldEqual(1);
            contextKey.Deserialize(1, serializationContext).ShouldEqual(1);

            var memento = contextKey.GetMemento()!;
            memento.TargetType.ShouldEqual(contextKey.GetType());
            memento.Preserve(serializationContext);
            ContextKeyField = MetadataContextKey.FromKey<int>("121");
            var restore = memento.Restore(serializationContext);
            restore.IsRestored.ShouldBeTrue();
            restore.Target.ShouldEqual(ContextKeyField);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromMemberShouldCreateMetadataKeyFromProperty(bool serializable)
        {
            var key = MetadataContextKey.FromMember<int>(GetType(), nameof(ContextKeyProperty), serializable);
            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            var contextKey = (key as ISerializableMetadataContextKey)!;
            if (!serializable)
            {
                contextKey.ShouldBeNull();
                return;
            }

            var serializationContext = new SerializationContext(DefaultMetadata);
            contextKey.CanSerialize(1, serializationContext).ShouldBeTrue();
            contextKey.Serialize(1, serializationContext).ShouldEqual(1);
            contextKey.Deserialize(1, serializationContext).ShouldEqual(1);

            var memento = contextKey.GetMemento()!;
            memento.TargetType.ShouldEqual(contextKey.GetType());
            memento.Preserve(serializationContext);
            ContextKeyProperty = MetadataContextKey.FromKey<int>("121");
            var restore = memento.Restore(serializationContext);
            restore.IsRestored.ShouldBeTrue();
            restore.Target.ShouldEqual(ContextKeyProperty);
        }

        [Fact]
        public void FromBuilderTest1()
        {
            var key = MetadataContextKey.Create<int>("test").Build();
            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            (key is ISerializableMetadataContextKey).ShouldBeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromBuilderTest2(bool serializable)
        {
            var builder = MetadataContextKey.Create<int>(GetType(), nameof(ContextKeyField));
            if (serializable)
                builder.Serializable();
            var key = builder.Build();

            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            var contextKey = (key as ISerializableMetadataContextKey)!;
            if (!serializable)
            {
                contextKey.ShouldBeNull();
                return;
            }

            var serializationContext = new SerializationContext(DefaultMetadata);
            contextKey.CanSerialize(1, serializationContext).ShouldBeTrue();
            contextKey.Serialize(1, serializationContext).ShouldEqual(1);
            contextKey.Deserialize(1, serializationContext).ShouldEqual(1);

            var memento = contextKey.GetMemento()!;
            memento.TargetType.ShouldEqual(contextKey.GetType());
            memento.Preserve(serializationContext);
            ContextKeyField = MetadataContextKey.FromKey<int>("121");
            var restore = memento.Restore(serializationContext);
            restore.IsRestored.ShouldBeTrue();
            restore.Target.ShouldEqual(ContextKeyField);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromBuilderTest3(bool serializable)
        {
            var builder = MetadataContextKey.Create<int>(GetType(), nameof(ContextKeyProperty));
            if (serializable)
                builder.Serializable();
            var key = builder.Build();

            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            var contextKey = (key as ISerializableMetadataContextKey)!;
            if (!serializable)
            {
                contextKey.ShouldBeNull();
                return;
            }

            var serializationContext = new SerializationContext(DefaultMetadata);
            contextKey.CanSerialize(1, serializationContext).ShouldBeTrue();
            contextKey.Serialize(1, serializationContext).ShouldEqual(1);
            contextKey.Deserialize(1, serializationContext).ShouldEqual(1);

            var memento = contextKey.GetMemento()!;
            memento.TargetType.ShouldEqual(contextKey.GetType());
            memento.Preserve(serializationContext);
            ContextKeyProperty = MetadataContextKey.FromKey<int>("121");
            var restore = memento.Restore(serializationContext);
            restore.IsRestored.ShouldBeTrue();
            restore.Target.ShouldEqual(ContextKeyProperty);
        }

        [Fact]
        public void FromBuilderShouldSetGetterSetterValidation()
        {
            int setterCount = 0;
            int getterCount = 0;
            var oldValue = 10;
            var newValue = 100;
            IMetadataContextKey<int>? contextKey = null;
            var builder = MetadataContextKey.Create<int>(GetType(), nameof(ContextKeyField));
            contextKey = builder.Setter((context, key, oldV, newV) =>
            {
                ++setterCount;
                context.ShouldEqual(DefaultMetadata);
                key.ShouldEqual(contextKey);
                oldV.ShouldEqual(oldValue);
                newV.ShouldEqual(newValue);
                return newV;
            }).Getter((context, key, arg3) =>
            {
                ++getterCount;
                context.ShouldEqual(DefaultMetadata);
                key.ShouldEqual(contextKey);
                arg3.ShouldEqual(newValue);
                return oldValue;
            }).WithValidation((context, key, arg3) =>
            {
                if (arg3 == int.MaxValue)
                    throw new NotSupportedException();
            }).Build();

            contextKey.GetValue(DefaultMetadata, newValue).ShouldEqual(oldValue);
            getterCount.ShouldEqual(1);

            contextKey.SetValue(DefaultMetadata, oldValue, newValue).ShouldEqual(newValue);
            setterCount.ShouldEqual(1);

            ShouldThrow<NotSupportedException>(() => contextKey.SetValue(DefaultMetadata, oldValue, int.MaxValue));
        }

        [Fact]
        public void FromBuilderShouldSetDefaultValue()
        {
            var key = MetadataContextKey.Create<int>(GetType(), nameof(ContextKeyField)).DefaultValue(int.MinValue).Build();
            key.GetDefaultValue(DefaultMetadata, 0).ShouldEqual(int.MinValue);

            int invokeCount = 0;
            key = MetadataContextKey.Create<int>(GetType(), nameof(ContextKeyField)).DefaultValue((context, contextKey, arg3) =>
            {
                ++invokeCount;
                context.ShouldEqual(DefaultMetadata);
                contextKey.ShouldEqual(key);
                return int.MinValue;
            }).Build();
            key.GetDefaultValue(DefaultMetadata, 0).ShouldEqual(int.MinValue);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void FromBuilderShouldSetSerializableConverter()
        {
            int canSerializeCount = 0;
            int serializeCount = 0;
            int deserializeCount = 0;
            int currentValue = 100;
            object? serializeValue = new object();
            object? deserializeValue = new object();
            bool canSerializeValue = false;
            var serializationContext = new SerializationContext(DefaultMetadata);
            IMetadataContextKey<int>? key = null;
            key = MetadataContextKey
                .Create<int>(GetType(), nameof(ContextKeyField))
                .Serializable((contextKey, o, arg3) =>
                {
                    ++canSerializeCount;
                    contextKey.ShouldEqual(key);
                    o.ShouldEqual(currentValue);
                    arg3.ShouldEqual(serializationContext);
                    return canSerializeValue;
                })
                .SerializableConverter((contextKey, o, arg3) =>
                {
                    ++serializeCount;
                    contextKey.ShouldEqual(key);
                    o.ShouldEqual(currentValue);
                    arg3.ShouldEqual(serializationContext);
                    return serializeValue;
                }, (contextKey, o, arg3) =>
                {
                    ++deserializeCount;
                    contextKey.ShouldEqual(key);
                    o.ShouldEqual(currentValue);
                    arg3.ShouldEqual(serializationContext);
                    return deserializeValue;
                }).Build();

            var serKey = (ISerializableMetadataContextKey)key;
            serKey.CanSerialize(currentValue, serializationContext).ShouldEqual(canSerializeValue);
            canSerializeCount.ShouldEqual(1);

            canSerializeValue = true;
            serKey.CanSerialize(currentValue, serializationContext).ShouldEqual(canSerializeValue);
            canSerializeCount.ShouldEqual(2);

            serKey.Serialize(currentValue, serializationContext).ShouldEqual(serializeValue);
            serializeCount.ShouldEqual(1);

            serKey.Deserialize(currentValue, serializationContext).ShouldEqual(deserializeValue);
            deserializeCount.ShouldEqual(1);
        }
        #endregion
    }
}