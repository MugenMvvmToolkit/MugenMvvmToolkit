using System;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Infrastructure.Metadata
{
    public abstract class MetadataContextKey : IMetadataContextKey
    {
        #region Fields

        private readonly string? _fieldOrPropertyName;
        private readonly Type? _type;
        private IMemento? _memento;

        #endregion

        #region Constructors

        protected MetadataContextKey(string key, Type? type, string? fieldOrPropertyName)
        {
            _type = type;
            _fieldOrPropertyName = fieldOrPropertyName;
            Key = key;
        }

        #endregion

        #region Properties

        public string Key { get; }

        #endregion

        #region Implementation of interfaces

        public abstract object? ToSerializableValue(object? item, ISerializationContext serializationContext);

        public abstract bool CanSerializeValue(object? item, ISerializationContext serializationContext);

        public virtual IMemento? GetMemento()
        {
            if (_memento == null && _type != null && !string.IsNullOrEmpty(_fieldOrPropertyName))
                _memento = StaticMemberMemento.Create(this, _type, _fieldOrPropertyName);
            return _memento;
        }

        public virtual bool Equals(IMetadataContextKey other)
        {
            return string.Equals(Key, other.Key);
        }

        #endregion

        #region Methods

        public static IMetadataContextKey<T> FromKey<T>(string key)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new MetadataContextKeyInternal<T>(key, null, null);
        }

        public static IMetadataContextKey<T> FromMember<T>(Type declaredType, string fieldOrPropertyName)
        {
            var key = declaredType.Name + declaredType.FullName.Length + fieldOrPropertyName;
            return new MetadataContextKeyInternal<T>(key, declaredType, fieldOrPropertyName);
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IMetadataContextKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        #endregion

        #region Nested types

        public struct Builder<T>
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
            private Func<IMetadataContextKey<T>, object?, ISerializationContext, object?> _serializableConverter;

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
                _serializableConverter = null;
            }

            #endregion

            #region Methods

            public Builder<T> WithValidation(Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T> validateAction)
            {
                Should.BeValid(nameof(validateAction), _validateAction == null);
                _validateAction = validateAction;
                return this;
            }

            public Builder<T> Serializable()
            {
                return Serializable((k, o, context) => context.Serializer.CanSerialize(o?.GetType() ?? typeof(T), Default.MetadataContext));
            }

            public Builder<T> Serializable(Func<IMetadataContextKey<T>, object?, ISerializationContext, bool> canSerialize)
            {
                Should.NotBeNull(canSerialize, nameof(canSerialize));
                Should.BeValid(nameof(canSerialize), _canSerializeFunc == null);
                _canSerializeFunc = canSerialize;
                return this;
            }

            public Builder<T> SerializableConverter(Func<IMetadataContextKey<T>, object?, ISerializationContext, object?> serializableConverter)
            {
                Should.NotBeNull(serializableConverter, nameof(serializableConverter));
                Should.BeValid(nameof(serializableConverter), _serializableConverter == null);
                _serializableConverter = serializableConverter;
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

            public IMetadataContextKey<T> Build()
            {
                return new MetadataContextKeyInternal<T>(_key, _type, _fieldOrPropertyName)
                {
                    SetValueFunc = _setValueFunc,
                    ValidateAction = _validateAction,
                    GetValueFunc = _getValueFunc,
                    GetDefaultValueFunc = _getDefaultValueFunc,
                    CanSerializeFunc = _canSerializeFunc,
                    SerializableConverterFunc = _serializableConverter
                };
            }

            #endregion
        }

        private class MetadataContextKeyInternal<T> : MetadataContextKey, IMetadataContextKey<T>
        {
            #region Fields

            public Func<IMetadataContextKey<T>, object?, ISerializationContext, bool>? CanSerializeFunc;

            public Func<IMetadataContextKey<T>, object?, ISerializationContext, object?>? SerializableConverterFunc;

            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, T, T>? GetDefaultValueFunc;

            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T>? GetValueFunc;

            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?>? SetValueFunc;

            public Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T>? ValidateAction;

            #endregion

            #region Constructors

            protected internal MetadataContextKeyInternal(string key, Type? type, string? fieldOrPropertyName)
                : base(key, type, fieldOrPropertyName)
            {
            }

            #endregion

            #region Implementation of interfaces

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

            public override bool CanSerializeValue(object? item, ISerializationContext serializationContext)
            {
                if (CanSerializeFunc == null)
                    return false;
                return CanSerializeFunc(this, item, serializationContext);
            }

            public override object? ToSerializableValue(object? item, ISerializationContext serializationContext)
            {
                if (SerializableConverterFunc == null)
                    return item;
                return SerializableConverterFunc(this, item, serializationContext);
            }

            public T GetValue(IReadOnlyMetadataContext metadataContext, object? value)
            {
                if (GetValueFunc == null)
                    return (T)value;
                return GetValueFunc(metadataContext, this, value);
            }

            #endregion
        }

        #endregion
    }
}