using System;
using System.Collections.Generic;
using System.IO;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Metadata;
using MugenMvvm.Serialization;
using MugenMvvm.UnitTests.Serialization.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Metadata
{
    public class MetadataContextKeyTest : UnitTestBase
    {
        #region Fields

        public static IMetadataContextKey<int, int>? ContextKeyField;

        #endregion

        #region Properties

        public static IMetadataContextKey<int, int>? ContextKeyProperty { get; set; }

        #endregion

        #region Methods

        [Fact]
        public void FromKeyShouldCreateMetadataKeyFromString()
        {
            var meta = new Dictionary<string, object?>();
            var key = MetadataContextKey.FromKey<int, int>("test", meta);
            key.Metadata.ShouldEqual(meta);
            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            key.IsSerializable.ShouldBeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromMemberShouldCreateMetadataKeyFromField(bool serializable)
        {
            var meta = new Dictionary<string, object?>();
            var key = MetadataContextKey.FromMember<int, int>(GetType(), nameof(ContextKeyField), serializable, meta);
            key.Metadata.ShouldEqual(meta);
            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            key.IsSerializable.ShouldEqual(serializable);
            if (!serializable)
            {
                (key as IHasMemento)?.GetMemento().ShouldBeNull();
                return;
            }

            var serializationContext = new SerializationContext(Stream.Null, true, DefaultMetadata);
            var memento = ((IHasMemento)key).GetMemento()!;
            memento.TargetType.ShouldEqual(key.GetType());
            memento.Preserve(serializationContext);
            ContextKeyField = MetadataContextKey.FromKey<int, int>("121");
            var restore = memento.Restore(serializationContext);
            restore.IsRestored.ShouldBeTrue();
            restore.Target.ShouldEqual(ContextKeyField);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromMemberShouldCreateMetadataKeyFromProperty(bool serializable)
        {
            var meta = new Dictionary<string, object?>();
            var key = MetadataContextKey.FromMember<int, int>(GetType(), nameof(ContextKeyProperty), serializable, meta);
            key.Metadata.ShouldEqual(meta);
            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            key.IsSerializable.ShouldEqual(serializable);
            if (!serializable)
            {
                (key as IHasMemento)?.GetMemento().ShouldBeNull();
                return;
            }

            var serializationContext = new SerializationContext(Stream.Null, true, DefaultMetadata);
            var memento = ((IHasMemento)key).GetMemento()!;
            memento.TargetType.ShouldEqual(key.GetType());
            memento.Preserve(serializationContext);
            ContextKeyProperty = MetadataContextKey.FromKey<int, int>("121");
            var restore = memento.Restore(serializationContext);
            restore.IsRestored.ShouldBeTrue();
            restore.Target.ShouldEqual(ContextKeyProperty);
        }

        [Fact]
        public void FromBuilderTest0()
        {
            var key = MetadataContextKey.Create<int, int>("test").Build();
            key.Metadata.ShouldBeEmpty();
            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            key.IsSerializable.ShouldBeFalse();
        }

        [Fact]
        public void FromBuilderTest1()
        {
            var metaKey1 = "k1";
            var metaKey2 = "k2";
            var metaKey3 = metaKey1;
            object? metaValue1 = null;
            var metaValue2 = new object();
            var metaValue3 = new object();
            var key = MetadataContextKey
                .Create<int, int>("test")
                .WithMetadata(metaKey1, metaValue1)
                .WithMetadata(metaKey2, metaValue2)
                .WithMetadata(metaKey3, metaValue3)
                .Build();
            key.Metadata.Count.ShouldEqual(2);
            key.Metadata[metaKey1].ShouldEqual(metaValue3);
            key.Metadata[metaKey2].ShouldEqual(metaValue2);
            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            key.IsSerializable.ShouldBeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromBuilderTest2(bool serializable)
        {
            var builder = MetadataContextKey.Create<int, int>(GetType(), nameof(ContextKeyField));
            if (serializable)
                builder.Serializable();
            var key = builder.Build();

            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            key.IsSerializable.ShouldEqual(serializable);
            if (!serializable)
            {
                (key as IHasMemento)?.GetMemento().ShouldBeNull();
                return;
            }

            var serializationContext = new SerializationContext(Stream.Null, true, DefaultMetadata);
            var memento = ((IHasMemento)key).GetMemento()!;
            memento.TargetType.ShouldEqual(key.GetType());
            memento.Preserve(serializationContext);
            ContextKeyField = MetadataContextKey.FromKey<int, int>("121");
            var restore = memento.Restore(serializationContext);
            restore.IsRestored.ShouldBeTrue();
            restore.Target.ShouldEqual(ContextKeyField);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromBuilderTest3(bool serializable)
        {
            var builder = MetadataContextKey.Create<int, int>(GetType(), nameof(ContextKeyProperty));
            if (serializable)
                builder.Serializable();
            var key = builder.Build();

            key.GetValue(DefaultMetadata, 1).ShouldEqual(1);
            key.SetValue(DefaultMetadata, null, 2).ShouldEqual(2);
            key.GetDefaultValue(DefaultMetadata, 3).ShouldEqual(3);
            key.IsSerializable.ShouldEqual(serializable);

            if (!serializable)
            {
                (key as IHasMemento)?.GetMemento().ShouldBeNull();
                return;
            }

            var serializationContext = new SerializationContext(Stream.Null, true, DefaultMetadata);
            var memento = ((IHasMemento)key).GetMemento()!;
            memento.TargetType.ShouldEqual(key.GetType());
            memento.Preserve(serializationContext);
            ContextKeyProperty = MetadataContextKey.FromKey<int, int>("121");
            var restore = memento.Restore(serializationContext);
            restore.IsRestored.ShouldBeTrue();
            restore.Target.ShouldEqual(ContextKeyProperty);
        }

        [Fact]
        public void FromBuilderShouldSetGetterSetterValidation()
        {
            var setterCount = 0;
            var getterCount = 0;
            var oldValue = 10;
            var newValue = 100;
            IMetadataContextKey<int, int>? contextKey = null;
            var builder = MetadataContextKey.Create<int, int>(GetType(), nameof(ContextKeyField));
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
            var key = MetadataContextKey.Create<int, int>(GetType(), nameof(ContextKeyField)).DefaultValue(int.MinValue).Build();
            key.GetDefaultValue(DefaultMetadata, 0).ShouldEqual(int.MinValue);

            var invokeCount = 0;
            key = MetadataContextKey.Create<int, int>(GetType(), nameof(ContextKeyField)).DefaultValue((context, contextKey, arg3) =>
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
        public void FromBuilderShouldSetCustomMemento()
        {
            var memento = new TestMemento();
            var key = MetadataContextKey.Create<int, int>("k").WithMemento(contextKey => memento).Serializable().Build();
            ((IHasMemento)key).GetMemento().ShouldEqual(memento);
        }

        #endregion
    }
}