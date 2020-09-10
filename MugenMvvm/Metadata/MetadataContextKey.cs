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
    public abstract class MetadataContextKey : IMetadataContextKey, IHasId<string>
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

        public static IMetadataContextKey<TGet, TSet> FromKey<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string key, IReadOnlyDictionary<string, object?>? metadata = null) => FromKey<TGet, TSet>(key, metadata);

        public static IMetadataContextKey<TGet, TSet> FromKey<TGet, TSet>(string key, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new MetadataContextKeyInternal<TGet, TSet>(key, false, metadata, null, null);
        }

        public static IMetadataContextKey<TGet, TSet> FromMember<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, Type declaredType, string fieldOrPropertyName, bool serializable = false,
            IReadOnlyDictionary<string, object?>? metadata = null) => FromMember<TGet, TSet>(declaredType, fieldOrPropertyName, serializable, metadata);

        public static IMetadataContextKey<TGet, TSet> FromMember<TGet, TSet>(Type declaredType, string fieldOrPropertyName, bool serializable = false, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            Should.NotBeNull(declaredType, nameof(declaredType));
            Should.NotBeNullOrEmpty(fieldOrPropertyName, nameof(fieldOrPropertyName));
            return new MetadataContextKeyInternal<TGet, TSet>(GenerateKey(declaredType, fieldOrPropertyName), serializable, metadata, declaredType, fieldOrPropertyName);
        }

        public static Builder<TGet, TSet> Create<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, Type declaredType, string fieldOrPropertyName, string? key = null) =>
            Create<TGet, TSet>(declaredType, fieldOrPropertyName, key);

        public static Builder<TGet, TSet> Create<TGet, TSet>(Type declaredType, string fieldOrPropertyName, string? key = null)
        {
            Should.NotBeNull(declaredType, nameof(declaredType));
            Should.NotBeNull(fieldOrPropertyName, nameof(fieldOrPropertyName));
            return Create<TGet, TSet>(string.IsNullOrEmpty(key) ? GenerateKey(declaredType, fieldOrPropertyName) : key!, declaredType, fieldOrPropertyName);
        }

        public static Builder<TGet, TSet> Create<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string key, Type? declaredType = null, string? fieldOrPropertyName = null) =>
            Create<TGet, TSet>(key, declaredType, fieldOrPropertyName);

        public static Builder<TGet, TSet> Create<TGet, TSet>(string key, Type? declaredType = null, string? fieldOrPropertyName = null)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new Builder<TGet, TSet>(key, declaredType, fieldOrPropertyName);
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
        public ref struct Builder<TGet, TSet>
        {
            #region Fields

            private readonly string _key;
            private readonly Type? _declaredType;
            private readonly string? _fieldOrPropertyName;

            private Dictionary<string, object?>? _metadata;
            private Action<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TSet>? _validateAction;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TGet, TGet>? _getDefaultValueFunc;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TGet>? _getValueFunc;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TSet, object?>? _setValueFunc;
            private Func<IMetadataContextKey<TGet, TSet>, IMemento?>? _getMementoFunc;
            private bool _hasDefaultValue;
            private TGet _defaultValue;
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
                Should.BeValid(nameof(defaultValue), !_hasDefaultValue);
                _hasDefaultValue = true;
                _defaultValue = defaultValue;
                return this;
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
                _isSerializable = true;
                return this;
            }

            public Builder<TGet, TSet> WithMemento(Func<IMetadataContextKey<TGet, TSet>, IMemento?> getMemento)
            {
                Should.NotBeNull(getMemento, nameof(getMemento));
                _getMementoFunc = getMemento;
                return this;
            }

            public IMetadataContextKey<TGet, TSet> Build()
            {
                if (_isSerializable && _getMementoFunc == null && (_declaredType == null || _fieldOrPropertyName == null))
                    ExceptionManager.ThrowMementoRequiredContextKey();
                return new MetadataContextKeyInternal<TGet, TSet>(_key, _isSerializable, _metadata, _declaredType, _fieldOrPropertyName)
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

            #endregion
        }

        private class MetadataContextKeyInternal<TGet, TSet> : MetadataContextKey, IMetadataContextKey<TGet, TSet>, IHasMemento
        {
            #region Fields

            private readonly Type? _declaredType;
            private readonly string? _fieldOrPropertyName;
            private IMemento? _memento;

            public TGet DefaultValue;
            public Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TGet, TGet>? GetDefaultValueFunc;
            public Func<IMetadataContextKey<TGet, TSet>, IMemento?>? GetMementoFunc;
            public Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TGet>? GetValueFunc;
            public bool HasDefaultValue;
            public Func<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, object?, TSet, object?>? SetValueFunc;
            public Action<IReadOnlyMetadataContext, IMetadataContextKey<TGet, TSet>, TSet>? ValidateAction;

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

            public TGet GetDefaultValue(IReadOnlyMetadataContext metadataContext, [AllowNull] TGet defaultValue)
            {
                if (GetDefaultValueFunc == null)
                    return HasDefaultValue ? DefaultValue : defaultValue!;
                return GetDefaultValueFunc(metadataContext, this, defaultValue!);
            }

            #endregion

            #region Methods

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