using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Internal;
using MugenMvvm.Serialization;

namespace MugenMvvm.Metadata
{
    public abstract class MetadataContextKey : IMetadataContextKey
    {
        #region Constructors

        protected MetadataContextKey(string key, bool isSerializable, IReadOnlyDictionary<string, object?>? metadata)
        {
            Id = key;
            IsSerializable = isSerializable;
            Metadata = metadata ?? Default.ReadOnlyDictionary<string, object?>();
        }

        #endregion

        #region Properties

        public string Id { get; }

        public bool IsSerializable { get; }

        public IReadOnlyDictionary<string, object?> Metadata { get; }

        #endregion

        #region Implementation of interfaces

        public bool Equals(IMetadataContextKey? other) => EqualsInternal(other);

        #endregion

        #region Methods

        public static IMetadataContextKey<T> FromKey<T>(IMetadataContextKey<T>? _, string key, IReadOnlyDictionary<string, object?>? metadata = null) => FromKey<T>(key, metadata);

        public static IMetadataContextKey<T> FromKey<T>(string key, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new MetadataContextKeyInternal<T>(key, false, metadata, null, null);
        }

        public static IMetadataContextKey<T> FromMember<T>(IMetadataContextKey<T>? _, Type declaredType, string fieldOrPropertyName, bool serializable = false,
            IReadOnlyDictionary<string, object?>? metadata = null) => FromMember<T>(declaredType, fieldOrPropertyName, serializable, metadata);

        public static IMetadataContextKey<T> FromMember<T>(Type declaredType, string fieldOrPropertyName, bool serializable = false, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            Should.NotBeNull(declaredType, nameof(declaredType));
            Should.NotBeNullOrEmpty(fieldOrPropertyName, nameof(fieldOrPropertyName));
            return new MetadataContextKeyInternal<T>(GenerateKey(declaredType, fieldOrPropertyName), serializable, metadata, declaredType, fieldOrPropertyName);
        }

        public static Builder<T> Create<T>(IMetadataContextKey<T>? _, Type declaredType, string fieldOrPropertyName, string? key = null) =>
            Create<T>(declaredType, fieldOrPropertyName, key);

        public static Builder<T> Create<T>(Type declaredType, string fieldOrPropertyName, string? key = null)
        {
            Should.NotBeNull(declaredType, nameof(declaredType));
            Should.NotBeNull(fieldOrPropertyName, nameof(fieldOrPropertyName));
            return Create<T>(string.IsNullOrEmpty(key) ? GenerateKey(declaredType, fieldOrPropertyName) : key!, declaredType, fieldOrPropertyName);
        }

        public static Builder<T> Create<T>(IMetadataContextKey<T>? _, string key, Type? declaredType = null, string? fieldOrPropertyName = null) =>
            Create<T>(key, declaredType, fieldOrPropertyName);

        public static Builder<T> Create<T>(string key, Type? declaredType = null, string? fieldOrPropertyName = null)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new Builder<T>(key, declaredType, fieldOrPropertyName);
        }

        public sealed override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IMetadataContextKey other && EqualsInternal(other);
        }

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => Id;

        protected virtual bool EqualsInternal(IMetadataContextKey? other)
        {
            if (ReferenceEquals(other, this))
                return true;
            return other is IHasId<string> hasStringId && string.Equals(Id, hasStringId.Id);
        }

        private static string GenerateKey(Type declaredType, string fieldOrPropertyName)
            => declaredType.Name + declaredType.FullName!.Length.ToString(CultureInfo.InvariantCulture) + fieldOrPropertyName +
               declaredType.AssemblyQualifiedName!.Length.ToString(CultureInfo.InvariantCulture);

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public ref struct Builder<T>
        {
            #region Fields

            private readonly string _key;
            private readonly Type? _declaredType;
            private readonly string? _fieldOrPropertyName;

            private Dictionary<string, object?>? _metadata;
            private Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T>? _validateAction;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, T, T>? _getDefaultValueFunc;
            private Delegate? _getValueFunc;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?>? _setValueFunc;
            private Func<IMetadataContextKey<T>, IMemento?>? _getMementoFunc;
            private bool _hasDefaultValue;
            private T _defaultValue;
            private bool _isSerializable;

            #endregion

            #region Constructors

            public Builder(string key, Type? declaredType, string? fieldOrPropertyName)
            {
                Should.NotBeNull(key, nameof(key));
                _key = key;
                _declaredType = declaredType;
                _fieldOrPropertyName = fieldOrPropertyName;
                _metadata = null;
                _validateAction = null;
                _getDefaultValueFunc = null;
                _getValueFunc = null;
                _setValueFunc = null;
                _getMementoFunc = null;
                _isSerializable = false;
                _hasDefaultValue = false;
                _defaultValue = default!;
            }

            #endregion

            #region Methods

            public Builder<T> WithMetadata(string key, object? value)
            {
                _metadata ??= new Dictionary<string, object?>();
                _metadata[key] = value;
                return this;
            }

            public Builder<T> WithValidation(Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T> validateAction)
            {
                Should.BeValid(nameof(validateAction), _validateAction == null);
                _validateAction = validateAction;
                return this;
            }

            public Builder<T> Getter(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, T> getter) => GetterInternal(getter);

            public Builder<T> Getter(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T> getter) => GetterInternal(getter);

            public Builder<T> Setter(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?> setter)
            {
                Should.NotBeNull(setter, nameof(setter));
                Should.BeValid(nameof(setter), _setValueFunc == null);
                _setValueFunc = setter;
                return this;
            }

            public Builder<T> DefaultValue(T defaultValue)
            {
                Should.BeValid(nameof(defaultValue), !_hasDefaultValue);
                _hasDefaultValue = true;
                _defaultValue = defaultValue;
                return this;
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
                _isSerializable = true;
                return this;
            }

            public Builder<T> WithMemento(Func<IMetadataContextKey<T>, IMemento?> getMemento)
            {
                Should.NotBeNull(getMemento, nameof(getMemento));
                _getMementoFunc = getMemento;
                return this;
            }

            public IMetadataContextKey<T> Build()
            {
                if (_isSerializable && _getMementoFunc == null && (_declaredType == null || _fieldOrPropertyName == null))
                    ExceptionManager.ThrowMementoRequiredContextKey();
                return new MetadataContextKeyInternal<T>(_key, _isSerializable, _metadata, _declaredType, _fieldOrPropertyName)
                {
                    SetValueFunc = _setValueFunc,
                    ValidateAction = _validateAction,
                    GetValueFunc = _getValueFunc,
                    GetDefaultValueFunc = _getDefaultValueFunc,
                    DefaultValue = _defaultValue,
                    HasDefaultValue = _hasDefaultValue,
                    GetMementoFunc = _getMementoFunc
                };
            }

            private Builder<T> GetterInternal(Delegate getter)
            {
                Should.NotBeNull(getter, nameof(getter));
                Should.BeValid(nameof(getter), _getValueFunc == null);
                _getValueFunc = getter;
                return this;
            }

            #endregion
        }

        private class MetadataContextKeyInternal<T> : MetadataContextKey, IMetadataContextKey<T>, IHasMemento
        {
            #region Fields

            private readonly Type? _declaredType;
            private readonly string? _fieldOrPropertyName;
            private IMemento? _memento;

            public T DefaultValue;
            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, T, T>? GetDefaultValueFunc;
            public Func<IMetadataContextKey<T>, IMemento?>? GetMementoFunc;
            public Delegate? GetValueFunc;
            public bool HasDefaultValue;
            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?>? SetValueFunc;
            public Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T>? ValidateAction;

            #endregion

            #region Constructors

            public MetadataContextKeyInternal(string key, bool isSerializable, IReadOnlyDictionary<string, object?>? metadata, Type? declaredType, string? fieldOrPropertyName)
                : base(key, isSerializable, metadata)
            {
                _declaredType = declaredType;
                _fieldOrPropertyName = fieldOrPropertyName;
                DefaultValue = default!;
            }

            #endregion

            #region Implementation of interfaces

            public IMemento? GetMemento()
            {
                if (IsSerializable)
                    return _memento ??= GetMementoInternal();
                return null;
            }

            public T GetValue(IReadOnlyMetadataContext metadataContext, object? rawValue) => GetValueInternal(metadataContext, rawValue, default!, false);

            public T GetValue(IReadOnlyMetadataContext metadataContext, object? rawValue, T value) => GetValueInternal(metadataContext, rawValue, value, true);

            public object? SetValue(IReadOnlyMetadataContext metadataContext, object? oldValue, T newValue)
            {
                ValidateAction?.Invoke(metadataContext, this, newValue);
                if (SetValueFunc == null)
                    return BoxingExtensions.Box(newValue);
                return SetValueFunc(metadataContext, this, oldValue, newValue);
            }

            public T GetDefaultValue(IReadOnlyMetadataContext metadataContext, [AllowNull] T defaultValue)
            {
                if (GetDefaultValueFunc == null)
                    return HasDefaultValue ? DefaultValue : defaultValue!;
                return GetDefaultValueFunc(metadataContext, this, defaultValue!);
            }

            #endregion

            #region Methods

            private T GetValueInternal(IReadOnlyMetadataContext metadataContext, object? rawValue, T value, bool hasValue)
            {
                if (GetValueFunc == null)
                {
                    if (hasValue && SetValueFunc == null)
                        return value;

                    if (rawValue == null)
                        return default!;
                    return (T) rawValue!;
                }

                if (GetValueFunc is Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, T> f)
                    return f(metadataContext, this, rawValue, value!);
                return ((Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T>) GetValueFunc).Invoke(metadataContext, this, rawValue);
            }

            private IMemento? GetMementoInternal()
            {
                if (GetMementoFunc == null)
                    return StaticMemberMemento.Create(this, _declaredType!, _fieldOrPropertyName!);
                return GetMementoFunc(this);
            }

            #endregion
        }

        #endregion
    }
}