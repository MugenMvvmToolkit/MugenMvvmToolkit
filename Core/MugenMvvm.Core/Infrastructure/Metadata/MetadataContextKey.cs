using System;
using System.Reflection;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Metadata
{
    public abstract class MetadataContextKey : IMetadataContextKey
    {
        #region Fields

        private readonly string? _fieldOrPropertyName;
        private readonly Type? _type;

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

        public abstract bool CanSerialize(object? item, ISerializationContext context);

        public virtual IMemento? GetMemento()
        {
            if (_type != null && !string.IsNullOrEmpty(_fieldOrPropertyName))
            {
                MemberInfo member = _type.GetFieldUnified(_fieldOrPropertyName, MemberFlags.StaticOnly);
                if (member == null)
                    member = _type.GetPropertyUnified(_fieldOrPropertyName, MemberFlags.StaticOnly);
                if (member == null)
                    return null;
                return new ContextKeyMemento(this, member);
            }

            return null;
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

            private readonly string? _fieldOrPropertyName;
            private readonly string _key;
            private readonly Type? _type;

            private Action<T>? _validateAction;
            private Func<IReadOnlyMetadataContext, T, T>? _getDefaultValueFunc;
            private Func<IReadOnlyMetadataContext, object?, T>? _getValueFunc;
            private Func<IReadOnlyMetadataContext, T, object?>? _setValueFunc;
            private Func<object?, ISerializationContext, bool>? _canSerializeFunc;

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
            }

            #endregion

            #region Methods

            public Builder<T> WithValidation(Action<T> validateAction)
            {
                Should.BeValid(nameof(validateAction), _validateAction == null);
                _validateAction = validateAction;
                return this;
            }

            public Builder<T> Serializable()
            {
                return Serializable((o, context) => true);
            }

            public Builder<T> Serializable(Func<object?, ISerializationContext, bool> canSerialize)
            {
                Should.NotBeNull(canSerialize, nameof(canSerialize));
                Should.BeValid(nameof(canSerialize), _canSerializeFunc == null);
                _canSerializeFunc = canSerialize;
                return this;
            }

            public Builder<T> Getter(Func<IReadOnlyMetadataContext, object?, T> getter)
            {
                Should.NotBeNull(getter, nameof(getter));
                Should.BeValid(nameof(getter), _getValueFunc == null);
                _getValueFunc = getter;
                return this;
            }

            public Builder<T> Setter(Func<IReadOnlyMetadataContext, T, object?> setter)
            {
                Should.NotBeNull(setter, nameof(setter));
                Should.BeValid(nameof(setter), _setValueFunc == null);
                _setValueFunc = setter;
                return this;
            }

            public Builder<T> DefaultValue(T defaultValue)
            {
                return DefaultValue((context, arg2) => defaultValue);
            }

            public Builder<T> DefaultValue(Func<IReadOnlyMetadataContext, T, T> getDefaultValue)
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
                    CanSerializeFunc = _canSerializeFunc
                };
            }

            #endregion
        }

        private class MetadataContextKeyInternal<T> : MetadataContextKey, IMetadataContextKey<T>
        {
            #region Fields

            public Func<object?, ISerializationContext, bool>? CanSerializeFunc;

            public Func<IReadOnlyMetadataContext, T, T>? GetDefaultValueFunc;

            public Func<IReadOnlyMetadataContext, object?, T>? GetValueFunc;

            public Func<IReadOnlyMetadataContext, T, object?>? SetValueFunc;

            public Action<T>? ValidateAction;

            #endregion

            #region Constructors

            protected internal MetadataContextKeyInternal(string key, Type? type, string? fieldOrPropertyName)
                : base(key, type, fieldOrPropertyName)
            {
            }

            #endregion

            #region Implementation of interfaces

            public object? SetValue(IReadOnlyMetadataContext metadataContext, T value)
            {
                if (SetValueFunc == null)
                    return value;
                return SetValueFunc(metadataContext, value);
            }

            public T GetDefaultValue(IReadOnlyMetadataContext metadataContext, T defaultValue)
            {
                if (GetDefaultValueFunc == null)
                    return defaultValue;
                return GetDefaultValueFunc(metadataContext, defaultValue);
            }

            public override bool CanSerialize(object? item, ISerializationContext context)
            {
                if (CanSerializeFunc == null)
                    return false;
                return CanSerializeFunc(item, context);
            }

            public T GetValue(IReadOnlyMetadataContext metadataContext, object? value)
            {
                if (GetValueFunc == null)
                    return (T)value;
                return GetValueFunc(metadataContext, value);
            }

            public void Validate(T item)
            {
                ValidateAction?.Invoke(item);
            }

            #endregion
        }

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public sealed class ContextKeyMemento : IMemento
        {
            #region Fields

            [DataMember(Name = "C")]
            internal MemberInfo? ConstantMember;

            [DataMember(Name = "T")]
            internal Type TargetTypeField;

            #endregion

            #region Constructors

            internal ContextKeyMemento()
            {
            }

            internal ContextKeyMemento(MetadataContextKey contextKey, MemberInfo member)
            {
                TargetTypeField = contextKey.GetType();
                ConstantMember = member;
            }

            #endregion

            #region Properties

            [IgnoreDataMember]
            public Type TargetType => TargetTypeField;

            #endregion

            #region Implementation of interfaces

            public void Preserve(ISerializationContext serializationContext)
            {
            }

            public IMementoResult Restore(ISerializationContext serializationContext)
            {
                object contextKey;
                if (ConstantMember is PropertyInfo propertyInfo)
                    contextKey = propertyInfo.GetValue(null);
                else
                    contextKey = ((FieldInfo)ConstantMember).GetValue(null);
                if (contextKey == null)
                    return MementoResult.Unrestored;
                return new MementoResult(contextKey, serializationContext.Metadata);
            }

            #endregion
        }

        #endregion
    }
}