using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Serialization;

namespace MugenMvvm.Metadata
{
    public abstract class MetadataContextKey : IMetadataContextKey, IHasId<string>
    {
        #region Constructors

        protected MetadataContextKey(string key, IReadOnlyDictionary<string, object?>? metadata)
        {
            Id = key;
            Metadata = metadata ?? Default.ReadOnlyDictionary<string, object?>();
        }

        #endregion

        #region Properties

        public string Id { get; }

        public IReadOnlyDictionary<string, object?> Metadata { get; }

        #endregion

        #region Implementation of interfaces

        public bool Equals(IMetadataContextKey other)
        {
            return EqualsInternal(other);
        }

        #endregion

        #region Methods

        public static IMetadataContextKey<TGet, TSet> FromKey<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string key, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            return FromKey<TGet, TSet>(key, metadata);
        }

        public static IMetadataContextKey<TGet, TSet> FromKey<TGet, TSet>(string key, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new MetadataContextKeyInternal<TGet, TSet>(key, metadata);
        }

        public static IMetadataContextKey<TGet, TSet> FromMember<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, Type declaredType, string fieldOrPropertyName, bool serializable = false, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            return FromMember<TGet, TSet>(declaredType, fieldOrPropertyName, serializable, metadata);
        }

        public static IMetadataContextKey<TGet, TSet> FromMember<TGet, TSet>(Type declaredType, string fieldOrPropertyName, bool serializable = false, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            var key = declaredType.Name + declaredType.FullName.Length.ToString(CultureInfo.InvariantCulture) + fieldOrPropertyName;
            if (serializable)
            {
                return new SerializableMetadataContextKey<TGet, TSet>(key, declaredType, fieldOrPropertyName, metadata)
                {
                    CanSerializeFunc = (_, __, ___) => true
                };
            }

            return new MetadataContextKeyInternal<TGet, TSet>(key, metadata);
        }

        public static Builder<TGet, TSet> Create<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string key, Type? declaredType = null, string? fieldOrPropertyName = null)
        {
            return Create<TGet, TSet>(key, declaredType, fieldOrPropertyName);
        }

        public static Builder<TGet, TSet> Create<TGet, TSet>(string key, Type? declaredType = null, string? fieldOrPropertyName = null)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new Builder<TGet, TSet>(key, declaredType, fieldOrPropertyName);
        }

        public static Builder<TGet, TSet> Create<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, Type declaredType, string fieldOrPropertyName, string? key = null)
        {
            return Create<TGet, TSet>(declaredType, fieldOrPropertyName, key);
        }

        public static Builder<TGet, TSet> Create<TGet, TSet>(Type declaredType, string fieldOrPropertyName, string? key = null)
        {
            Should.NotBeNull(declaredType, nameof(declaredType));
            Should.NotBeNull(fieldOrPropertyName, nameof(fieldOrPropertyName));
            if (string.IsNullOrEmpty(key))
                key = declaredType.Name + declaredType.FullName.Length.ToString(CultureInfo.InvariantCulture) + fieldOrPropertyName;
            return new Builder<TGet, TSet>(key!, declaredType, fieldOrPropertyName);
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
            if (ReferenceEquals(other, this))
                return true;
            return other is IHasId<string> hasStringId && string.Equals(Id, hasStringId.Id);
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public ref struct Builder<TGet, TSet>
        {
            #region Fields

            // ReSharper disable FieldCanBeMadeReadOnly.Local
            private string? _fieldOrPropertyName;
            private string _key;
            private Type? _type;
            // ReSharper restore FieldCanBeMadeReadOnly.Local

            private Dictionary<string, object?>? _metadata;
            private Action<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TSet>? _validateAction;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TGet, TGet>? _getDefaultValueFunc;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TGet>? _getValueFunc;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TSet, object?>? _setValueFunc;
            private Func<IMetadataContextKey<TGet, TSet>, object?, ISerializationContext, bool>? _canSerializeFunc;
            private Func<IMetadataContextKey<TGet, TSet>, object?, ISerializationContext, object?>? _serializeFunc;
            private Func<IMetadataContextKey<TGet, TSet>, object?, ISerializationContext, object?>? _deserializeFunc;

            #endregion

            #region Constructors

            public Builder(string key, Type? type, string? fieldOrPropertyName)
            {
                Should.NotBeNull(key, nameof(key));
                _key = key;
                _type = type;
                _fieldOrPropertyName = fieldOrPropertyName;
                _metadata = null;
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

            public Builder<TGet, TSet> WithMetadata(string key, object? value)
            {
                _metadata ??= new Dictionary<string, object?>();
                _metadata[key] = value;
                return this;
            }

            public Builder<TGet, TSet> WithValidation(Action<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TSet> validateAction)
            {
                Should.BeValid(nameof(validateAction), _validateAction == null);
                _validateAction = validateAction;
                return this;
            }

            public Builder<TGet, TSet> Getter(Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TGet> getter)
            {
                Should.NotBeNull(getter, nameof(getter));
                Should.BeValid(nameof(getter), _getValueFunc == null);
                _getValueFunc = getter;
                return this;
            }

            public Builder<TGet, TSet> Setter(Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TSet, object?> setter)
            {
                Should.NotBeNull(setter, nameof(setter));
                Should.BeValid(nameof(setter), _setValueFunc == null);
                _setValueFunc = setter;
                return this;
            }

            public Builder<TGet, TSet> DefaultValue(TGet defaultValue)
            {
                return DefaultValue((context, key, arg2) => defaultValue);
            }

            public Builder<TGet, TSet> DefaultValue(Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TGet, TGet> getDefaultValue)
            {
                Should.NotBeNull(getDefaultValue, nameof(getDefaultValue));
                Should.BeValid(nameof(getDefaultValue), _getDefaultValueFunc == null);
                _getDefaultValueFunc = getDefaultValue;
                return this;
            }

            public Builder<TGet, TSet> Serializable()
            {
                return Serializable((_, __, ___) => true);
            }

            public Builder<TGet, TSet> Serializable(Func<IMetadataContextKey<TGet, TSet>, object?, ISerializationContext, bool> canSerialize)
            {
                Should.NotBeNull(canSerialize, nameof(canSerialize));
                Should.BeValid(nameof(canSerialize), _canSerializeFunc == null);
                _canSerializeFunc = canSerialize;
                return this;
            }

            public Builder<TGet, TSet> SerializableConverter(Func<IMetadataContextKey<TGet, TSet>, object?, ISerializationContext, object?> serialize,
                Func<IMetadataContextKey<TGet, TSet>, object?, ISerializationContext, object?> deserialize)
            {
                Should.NotBeNull(serialize, nameof(serialize));
                Should.NotBeNull(deserialize, nameof(deserialize));
                Should.BeValid(nameof(serialize), _serializeFunc == null);
                Should.BeValid(nameof(deserialize), _deserializeFunc == null);
                _serializeFunc = serialize;
                _deserializeFunc = deserialize;
                return this;
            }

            public IMetadataContextKey<TGet, TSet> Build()
            {
                if (_serializeFunc == null && _deserializeFunc == null && _canSerializeFunc == null)
                {
                    return new MetadataContextKeyInternal<TGet, TSet>(_key, _metadata)
                    {
                        SetValueFunc = _setValueFunc,
                        ValidateAction = _validateAction,
                        GetValueFunc = _getValueFunc,
                        GetDefaultValueFunc = _getDefaultValueFunc
                    };
                }

                if (_canSerializeFunc == null)
                    Serializable();
                return new SerializableMetadataContextKey<TGet, TSet>(_key, _type, _fieldOrPropertyName, _metadata)
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

        private class MetadataContextKeyInternal<TGet, TSet> : MetadataContextKey, IMetadataContextKey<TGet, TSet>
        {
            #region Fields

            public Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TGet, TGet>? GetDefaultValueFunc;
            public Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TGet>? GetValueFunc;
            public Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TSet, object?>? SetValueFunc;
            public Action<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TSet>? ValidateAction;

            #endregion

            #region Constructors

            public MetadataContextKeyInternal(string key, IReadOnlyDictionary<string, object?>? metadata)
                : base(key, metadata)
            {
            }

            #endregion

            #region Implementation of interfaces

            public TGet GetValue(IReadOnlyMetadataContext metadataContext, object? value)
            {
                if (GetValueFunc == null)
                {
                    if (value == null)
                        return default!;
                    return (TGet)value!;
                }

                return GetValueFunc(metadataContext, this, value);
            }

            public object? SetValue(IReadOnlyMetadataContext metadataContext, object? oldValue, TSet newValue)
            {
                ValidateAction?.Invoke(metadataContext, this, newValue);
                if (SetValueFunc == null)
                    return BoxingExtensions.Box(newValue);
                return SetValueFunc(metadataContext, this, oldValue, newValue);
            }

            public TGet GetDefaultValue(IReadOnlyMetadataContext metadataContext, TGet defaultValue)
            {
                if (GetDefaultValueFunc == null)
                    return defaultValue;
                return GetDefaultValueFunc(metadataContext, this, defaultValue);
            }

            #endregion
        }

        private sealed class SerializableMetadataContextKey<TGet, TSet> : MetadataContextKeyInternal<TGet, TSet>, ISerializableMetadataContextKey, IHasMemento
        {
            #region Fields

            private readonly string? _fieldOrPropertyName;
            private readonly Type? _type;
            private IMemento? _memento;

            public Func<IMetadataContextKey<TGet, TSet>, object?, ISerializationContext, bool>? CanSerializeFunc;
            public Func<IMetadataContextKey<TGet, TSet>, object?, ISerializationContext, object?>? DeserializeFunc;
            public Func<IMetadataContextKey<TGet, TSet>, object?, ISerializationContext, object?>? SerializeFunc;

            #endregion

            #region Constructors

            public SerializableMetadataContextKey(string key, Type? type, string? fieldOrPropertyName, IReadOnlyDictionary<string, object?>? metadata)
                : base(key, metadata)
            {
                _type = type;
                _fieldOrPropertyName = fieldOrPropertyName;
            }

            #endregion

            #region Implementation of interfaces

            public IMemento? GetMemento()
            {
                if (_memento == null && _type != null && !string.IsNullOrEmpty(_fieldOrPropertyName))
                    _memento = StaticMemberMemento.Create(this, _type, _fieldOrPropertyName!);
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