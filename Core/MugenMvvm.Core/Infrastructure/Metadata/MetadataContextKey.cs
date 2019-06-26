using System;
using System.Runtime.InteropServices;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Infrastructure.Metadata
{
    public abstract class MetadataContextKey : IMetadataContextKey, IHasId<string>
    {
        #region Constructors

        protected MetadataContextKey(string key)
        {
            Id = key;
        }

        #endregion

        #region Properties

        public string Id { get; }

        #endregion

        #region Implementation of interfaces

        public bool Equals(IMetadataContextKey other)
        {
            return EqualsInternal(other);
        }

        #endregion

        #region Methods

        public static IMetadataContextKey<T> FromKey<T>(string key)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new MetadataContextKeyInternal<T>(key);
        }

        public static IMetadataContextKey<T> FromMember<T>(Type declaredType, string fieldOrPropertyName, bool serializable = false)
        {
            var key = declaredType.Name + declaredType.FullName.Length + fieldOrPropertyName;
            if (serializable)
            {
                return new SerializableMetadataContextKey<T>(key, declaredType, fieldOrPropertyName)
                {
                    CanSerializeFunc = (_, __, ___) => true
                };
            }
            return new MetadataContextKeyInternal<T>(key);
        }

        public static Builder<T> Create<T>(string key, Type? declaredType = null, string? fieldOrPropertyName = null)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new Builder<T>(key, declaredType, fieldOrPropertyName);
        }

        public static Builder<T> Create<T>(Type declaredType, string fieldOrPropertyName, string? key = null)
        {
            Should.NotBeNull(declaredType, nameof(declaredType));
            Should.NotBeNull(fieldOrPropertyName, nameof(fieldOrPropertyName));
            if (string.IsNullOrEmpty(key))
                key = declaredType.Name + declaredType.FullName.Length + fieldOrPropertyName;
            return new Builder<T>(key, declaredType, fieldOrPropertyName);
        }

        public sealed override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IMetadataContextKey other && EqualsInternal(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id;
        }

        protected virtual bool EqualsInternal(IMetadataContextKey other)
        {
            return other is IHasId<string> hasStringId && string.Equals(Id, hasStringId.Id);
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public ref struct Builder<T>
        {
            #region Fields

            // ReSharper disable FieldCanBeMadeReadOnly.Local
            private string? _fieldOrPropertyName;
            private string _key;

            private Type? _type;
            // ReSharper restore FieldCanBeMadeReadOnly.Local

            private Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T>? _validateAction;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, T, T>? _getDefaultValueFunc;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T>? _getValueFunc;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?>? _setValueFunc;
            private Func<IMetadataContextKey<T>, object?, ISerializationContext, bool>? _canSerializeFunc;
            private Func<IMetadataContextKey<T>, object?, ISerializationContext, object?>? _serializeFunc;
            private Func<IMetadataContextKey<T>, object?, ISerializationContext, object?>? _deserializeFunc;

            #endregion

            #region Constructors

            public Builder(string key, Type? type, string? fieldOrPropertyName)
            {
                _key = key;
                _type = type;
                _fieldOrPropertyName = fieldOrPropertyName;
                _validateAction = null;
                _getDefaultValueFunc = null;
                _getValueFunc = null;
                _setValueFunc = null;
                _canSerializeFunc = null;
                _serializeFunc = null;
                _deserializeFunc = null;
            }

            #endregion

            #region Methods

            public Builder<T> WithValidation(Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T> validateAction)
            {
                Should.BeValid(nameof(validateAction), _validateAction == null);
                _validateAction = validateAction;
                return this;
            }

            public Builder<T> Getter(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T> getter)
            {
                Should.NotBeNull(getter, nameof(getter));
                Should.BeValid(nameof(getter), _getValueFunc == null);
                _getValueFunc = getter;
                return this;
            }

            public Builder<T> Setter(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?> setter)
            {
                Should.NotBeNull(setter, nameof(setter));
                Should.BeValid(nameof(setter), _setValueFunc == null);
                _setValueFunc = setter;
                return this;
            }

            public Builder<T> DefaultValue(T defaultValue)
            {
                return DefaultValue((context, key, arg2) => defaultValue);
            }

            public Builder<T> DefaultValue(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, T, T> getDefaultValue)
            {
                Should.NotBeNull(getDefaultValue, nameof(getDefaultValue));
                Should.BeValid(nameof(getDefaultValue), _getDefaultValueFunc == null);
                _getDefaultValueFunc = getDefaultValue;
                return this;
            }

            public Builder<T> Serializable()
            {
                return Serializable((_, __, ___) => true);
            }

            public Builder<T> Serializable(Func<IMetadataContextKey<T>, object?, ISerializationContext, bool> canSerialize)
            {
                Should.NotBeNull(canSerialize, nameof(canSerialize));
                Should.BeValid(nameof(canSerialize), _canSerializeFunc == null);
                _canSerializeFunc = canSerialize;
                return this;
            }

            public Builder<T> SerializableConverter(Func<IMetadataContextKey<T>, object?, ISerializationContext, object?> serialize,
                Func<IMetadataContextKey<T>, object?, ISerializationContext, object?> deserialize)
            {
                Should.NotBeNull(serialize, nameof(serialize));
                Should.NotBeNull(deserialize, nameof(deserialize));
                Should.BeValid(nameof(serialize), _serializeFunc == null);
                Should.BeValid(nameof(deserialize), _deserializeFunc == null);
                _serializeFunc = serialize;
                _deserializeFunc = deserialize;
                return this;
            }

            public IMetadataContextKey<T> Build()
            {
                if (_serializeFunc == null && _deserializeFunc == null && _canSerializeFunc == null)
                {
                    return new MetadataContextKeyInternal<T>(_key)
                    {
                        SetValueFunc = _setValueFunc,
                        ValidateAction = _validateAction,
                        GetValueFunc = _getValueFunc,
                        GetDefaultValueFunc = _getDefaultValueFunc
                    };
                }

                if (_canSerializeFunc == null)
                    Serializable();
                return new SerializableMetadataContextKey<T>(_key, _type, _fieldOrPropertyName)
                {
                    SetValueFunc = _setValueFunc,
                    ValidateAction = _validateAction,
                    GetValueFunc = _getValueFunc,
                    GetDefaultValueFunc = _getDefaultValueFunc,
                    CanSerializeFunc = _canSerializeFunc,
                    DeserializeFunc = _deserializeFunc,
                    SerializeFunc = _serializeFunc
                };
            }

            #endregion
        }

        private class MetadataContextKeyInternal<T> : MetadataContextKey, IMetadataContextKey<T>
        {
            #region Fields

            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, T, T>? GetDefaultValueFunc;

            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T>? GetValueFunc;

            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?>? SetValueFunc;

            public Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T>? ValidateAction;

            #endregion

            #region Constructors

            public MetadataContextKeyInternal(string key)
                : base(key)
            {
            }

            #endregion

            #region Implementation of interfaces

            public T GetValue(IReadOnlyMetadataContext metadataContext, object? value)
            {
                if (GetValueFunc == null)
                    return (T)value!;
                return GetValueFunc(metadataContext, this, value);
            }

            public object? SetValue(IReadOnlyMetadataContext metadataContext, object? oldValue, T newValue)
            {
                ValidateAction?.Invoke(metadataContext, this, newValue);
                if (SetValueFunc == null)
                    return newValue;
                return SetValueFunc(metadataContext, this, oldValue, newValue);
            }

            public T GetDefaultValue(IReadOnlyMetadataContext metadataContext, T defaultValue)
            {
                if (GetDefaultValueFunc == null)
                    return defaultValue;
                return GetDefaultValueFunc(metadataContext, this, defaultValue);
            }

            #endregion
        }

        private sealed class SerializableMetadataContextKey<T> : MetadataContextKeyInternal<T>, ISerializableMetadataContextKey
        {
            #region Fields

            private readonly string? _fieldOrPropertyName;
            private readonly Type? _type;
            private IMemento? _memento;

            public Func<IMetadataContextKey<T>, object?, ISerializationContext, bool>? CanSerializeFunc;
            public Func<IMetadataContextKey<T>, object?, ISerializationContext, object?>? DeserializeFunc;
            public Func<IMetadataContextKey<T>, object?, ISerializationContext, object?>? SerializeFunc;

            #endregion

            #region Constructors

            public SerializableMetadataContextKey(string key, Type? type, string? fieldOrPropertyName)
                : base(key)
            {
                _type = type;
                _fieldOrPropertyName = fieldOrPropertyName;
            }

            #endregion

            #region Implementation of interfaces

            public IMemento? GetMemento()
            {
                if (_memento == null && _type != null && !string.IsNullOrEmpty(_fieldOrPropertyName))
                    _memento = StaticMemberMemento.Create(this, _type, _fieldOrPropertyName);
                return _memento;
            }

            public bool CanSerialize(object? item, ISerializationContext serializationContext)
            {
                if (CanSerializeFunc == null)
                    return false;
                return CanSerializeFunc(this, item, serializationContext);
            }

            public object? Serialize(object? item, ISerializationContext serializationContext)
            {
                if (SerializeFunc == null)
                    return item;
                return SerializeFunc(this, item, serializationContext);
            }

            public object? Deserialize(object? item, ISerializationContext serializationContext)
            {
                if (DeserializeFunc == null)
                    return item;
                return DeserializeFunc(this, item, serializationContext);
            }

            #endregion
        }

        #endregion
    }
}