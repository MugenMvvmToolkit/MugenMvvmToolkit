using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;
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
        protected MetadataContextKey(string key, bool isSerializable, IReadOnlyDictionary<string, object?>? metadata)
        {
            Id = key;
            IsSerializable = isSerializable;
            Metadata = metadata ?? Default.ReadOnlyDictionary<string, object?>();
        }

        public abstract Type ValueType { get; }

        public string Id { get; }

        public bool IsSerializable { get; }

        public IReadOnlyDictionary<string, object?> Metadata { get; }

        public static IMetadataContextKey<T> FromKey<T>(IMetadataContextKey<T>? _, string key, IReadOnlyDictionary<string, object?>? metadata = null) => FromKey<T>(key, metadata);

        public static IMetadataContextKey<T> FromKey<T>(string key, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new MetadataContextKeyInternal<T>(key, false, metadata, null, null);
        }

        public static IMetadataContextKey<T> FromMember<T>(IMetadataContextKey<T>? _, Type declaredType, string fieldOrPropertyName, bool serializable = false,
            IReadOnlyDictionary<string, object?>? metadata = null) => FromMember<T>(declaredType, fieldOrPropertyName, serializable, metadata);

        public static IMetadataContextKey<T> FromMember<T>(Type declaredType, string fieldOrPropertyName, bool serializable = false,
            IReadOnlyDictionary<string, object?>? metadata = null)
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

        public bool Equals(IMetadataContextKey? other) => EqualsInternal(other);

        protected virtual bool EqualsInternal(IMetadataContextKey? other)
        {
            if (ReferenceEquals(other, this))
                return true;
            return other is IHasId<string> hasStringId && string.Equals(Id, hasStringId.Id);
        }

        private static string GenerateKey(Type declaredType, string fieldOrPropertyName)
        {
#if SPAN_API
            var length = declaredType.Name.Length + 4 + fieldOrPropertyName.Length + 4;
            if (length < 256)
            {
                Span<char> st = stackalloc char[length];
                declaredType.Name.AsSpan().CopyTo(st);
                declaredType.FullName!.Length.TryFormat(st.Slice(declaredType.Name.Length), out var written1, default, CultureInfo.InvariantCulture);
                fieldOrPropertyName.AsSpan().CopyTo(st.Slice(declaredType.Name.Length + written1));
                declaredType.AssemblyQualifiedName!.Length.TryFormat(st.Slice(declaredType.Name.Length + written1 + fieldOrPropertyName.Length), out var written2, default,
                    CultureInfo.InvariantCulture);
                return new string(st.Slice(0, declaredType.Name.Length + written1 + fieldOrPropertyName.Length + written2));
            }
#endif
            return declaredType.Name + declaredType.FullName!.Length.ToString(CultureInfo.InvariantCulture) + fieldOrPropertyName +
                   declaredType.AssemblyQualifiedName!.Length.ToString(CultureInfo.InvariantCulture);
        }

        [StructLayout(LayoutKind.Auto)]
        public ref struct Builder<T>
        {
            public readonly string Key;
            private readonly Type? _declaredType;
            private readonly string? _fieldOrPropertyName;

            private object? _buildCallbacks;
            private Dictionary<string, object?>? _metadata;
            private Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T>? _validateAction;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, T, T>? _getDefaultValueFunc;
            private Delegate? _getValueFunc;
            private Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?>? _setValueFunc;
            private Func<IMetadataContextKey<T>, IMemento?>? _getMementoFunc;
            private bool _hasDefaultValue;
            private T _defaultValue;
            private bool _isSerializable;

            public Builder(string key, Type? declaredType, string? fieldOrPropertyName)
            {
                Should.NotBeNull(key, nameof(key));
                Key = key;
                _declaredType = declaredType;
                _fieldOrPropertyName = fieldOrPropertyName;
                _metadata = null;
                _validateAction = null;
                _getDefaultValueFunc = null;
                _getValueFunc = null;
                _setValueFunc = null;
                _getMementoFunc = null;
                _buildCallbacks = null;
                _isSerializable = false;
                _hasDefaultValue = false;
                _defaultValue = default!;
            }

            public Builder<T> WithBuildCallback(Action<IMetadataContextKey<T>> callback)
            {
                Should.NotBeNull(callback, nameof(callback));
                var list = ItemOrListEditor<Action<IMetadataContextKey<T>>>.FromRawValue(_buildCallbacks);
                list.Add(callback);
                _buildCallbacks = list.GetRawValueInternal();
                return this;
            }

            public Builder<T> WithMetadata(string key, object? value)
            {
                _metadata ??= new Dictionary<string, object?>();
                _metadata[key] = value;
                return this;
            }

            public Builder<T> WithValidation(Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T> validateAction)
            {
                Should.BeValid(_validateAction == null, nameof(validateAction));
                _validateAction = validateAction;
                return this;
            }

            public Builder<T> Getter(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, T> getter) => GetterInternal(getter);

            public Builder<T> Getter(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T> getter) => GetterInternal(getter);

            public Builder<T> Setter(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?> setter)
            {
                Should.NotBeNull(setter, nameof(setter));
                Should.BeValid(_setValueFunc == null, nameof(setter));
                _setValueFunc = setter;
                return this;
            }

            public Builder<T> DefaultValue(T defaultValue)
            {
                Should.BeValid(!_hasDefaultValue, nameof(defaultValue));
                _hasDefaultValue = true;
                _defaultValue = defaultValue;
                return this;
            }

            public Builder<T> DefaultValue(Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, T, T> getDefaultValue)
            {
                Should.NotBeNull(getDefaultValue, nameof(getDefaultValue));
                Should.BeValid(_getDefaultValueFunc == null, nameof(getDefaultValue));
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
                var key = new MetadataContextKeyInternal<T>(Key, _isSerializable, _metadata, _declaredType, _fieldOrPropertyName)
                {
                    SetValueFunc = _setValueFunc,
                    ValidateAction = _validateAction,
                    GetValueFunc = _getValueFunc,
                    GetDefaultValueFunc = _getDefaultValueFunc,
                    DefaultValue = _defaultValue,
                    HasDefaultValue = _hasDefaultValue,
                    GetMementoFunc = _getMementoFunc
                };
                if (_buildCallbacks != null)
                {
                    foreach (var action in ItemOrIReadOnlyList.FromRawValue<Action<IMetadataContextKey<T>>>(_buildCallbacks))
                        action(key);
                }

                return key;
            }

            private Builder<T> GetterInternal(Delegate getter)
            {
                Should.NotBeNull(getter, nameof(getter));
                Should.BeValid(_getValueFunc == null, nameof(getter));
                _getValueFunc = getter;
                return this;
            }
        }

        private sealed class MetadataContextKeyInternal<T> : MetadataContextKey, IMetadataContextKey<T>, IHasMemento
        {
            public T DefaultValue;
            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, T, T>? GetDefaultValueFunc;
            public Func<IMetadataContextKey<T>, IMemento?>? GetMementoFunc;
            public Delegate? GetValueFunc;
            public bool HasDefaultValue;
            public Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, object?>? SetValueFunc;
            public Action<IReadOnlyMetadataContext, IMetadataContextKey<T>, T>? ValidateAction;

            private readonly Type? _declaredType;
            private readonly string? _fieldOrPropertyName;
            private IMemento? _memento;

            public MetadataContextKeyInternal(string key, bool isSerializable, IReadOnlyDictionary<string, object?>? metadata, Type? declaredType, string? fieldOrPropertyName)
                : base(key, isSerializable, metadata)
            {
                _declaredType = declaredType;
                _fieldOrPropertyName = fieldOrPropertyName;
                DefaultValue = default!;
            }

            public override Type ValueType => typeof(T);

            public IMemento? GetMemento()
            {
                if (IsSerializable)
                    return _memento ??= GetMementoInternal();
                return null;
            }

            public object? SetValue(IReadOnlyMetadataContext metadataContext, object? oldValue, T newValue)
            {
                ValidateAction?.Invoke(metadataContext, this, newValue);
                if (SetValueFunc == null)
                    return BoxingExtensions.Box(newValue);
                return SetValueFunc(metadataContext, this, oldValue, newValue);
            }

            public T GetValue(IReadOnlyMetadataContext metadataContext, object? rawValue) => GetValueInternal(metadataContext, rawValue, default!, false);

            public T GetValue(IReadOnlyMetadataContext metadataContext, object? rawValue, T value) => GetValueInternal(metadataContext, rawValue, value, true);

            public T GetDefaultValue(IReadOnlyMetadataContext metadataContext, T? defaultValue)
            {
                if (GetDefaultValueFunc == null)
                    return HasDefaultValue ? DefaultValue : defaultValue!;
                return GetDefaultValueFunc(metadataContext, this, defaultValue!);
            }

            private T GetValueInternal(IReadOnlyMetadataContext metadataContext, object? rawValue, T value, bool hasValue)
            {
                if (GetValueFunc == null)
                {
                    if (hasValue && SetValueFunc == null)
                        return value;

                    if (rawValue == null)
                        return default!;
                    return (T)rawValue!;
                }

                if (GetValueFunc is Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T, T> f)
                    return f(metadataContext, this, rawValue, value!);
                return ((Func<IReadOnlyMetadataContext, IMetadataContextKey<T>, object?, T>)GetValueFunc).Invoke(metadataContext, this, rawValue);
            }

            private IMemento? GetMementoInternal()
            {
                if (GetMementoFunc == null)
                    return StaticMemberMemento.Create(this, _declaredType!, _fieldOrPropertyName!);
                return GetMementoFunc(this);
            }
        }
    }
}