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
    public class MetadataContextKey : IMetadataContextKey
    {
        #region Fields

        private readonly Func<object?, ISerializationContext, bool>? _canSerialize;
        private readonly string? _fieldOrPropertyName;
        private readonly Type? _type;
        private readonly Action<object?>? _validateAction;

        #endregion

        #region Constructors

        protected MetadataContextKey(string key, Action<object?>? validateAction, Func<object?, ISerializationContext, bool>? canSerialize, Type? type, string? fieldOrPropertyName)
        {
            _validateAction = validateAction;
            _canSerialize = canSerialize;
            _type = type;
            _fieldOrPropertyName = fieldOrPropertyName;
            Key = key;
        }

        #endregion

        #region Properties

        public string Key { get; }

        #endregion

        #region Implementation of interfaces

        public static IMetadataContextKey<T> FromKey<T>(string key)
        {
            Should.NotBeNullOrEmpty(key, nameof(key));
            return new MetadataContextKeyInternal<T>(key, null, null, null, null);
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

        public IMemento? GetMemento()
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

        public void Validate(object? item)
        {
            _validateAction?.Invoke(item);
        }

        public bool CanSerialize(object? item, ISerializationContext context)
        {
            if (_canSerialize == null)
                return false;
            return _canSerialize(item, context);
        }

        public bool Equals(IMetadataContextKey other)
        {
            return string.Equals(Key, other.Key);
        }

        #endregion

        #region Methods

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

            private Func<object?, ISerializationContext, bool>? _canSerialize;
            private Action<object?>? _validateAction;

            #endregion

            #region Constructors

            public Builder(string key, Type? type, string? fieldOrPropertyName)
            {
                _key = key;
                _type = type;
                _fieldOrPropertyName = fieldOrPropertyName;
                _canSerialize = null;
                _validateAction = null;
            }

            #endregion

            #region Methods

            public Builder<T> NotNull()
            {
                return SetValidation(value => Should.NotBeNull(value, nameof(value)));
            }

            public Builder<T> WithValidation(Action<T> validateAction)
            {
                Should.NotBeNull(validateAction, nameof(validateAction));
                return SetValidation(o => validateAction((T)o));
            }

            public Builder<T> Serializable()
            {
                return SetSerializable((o, context) => true);
            }

            public Builder<T> Serializable(Func<T, ISerializationContext, bool> canSerialize)
            {
                Should.NotBeNull(canSerialize, nameof(canSerialize));
                return SetSerializable((o, context) => canSerialize((T)o, context));
            }

            public IMetadataContextKey<T> Build()
            {
                return new MetadataContextKeyInternal<T>(_key, _validateAction, _canSerialize, _type, _fieldOrPropertyName);
            }

            private Builder<T> SetSerializable(Func<object?, ISerializationContext, bool> canSerialize)
            {
                Should.BeValid(nameof(canSerialize), _canSerialize == null);
                _canSerialize = canSerialize;
                return this;
            }

            private Builder<T> SetValidation(Action<object?> validateAction)
            {
                Should.BeValid(nameof(validateAction), _validateAction == null);
                _validateAction = validateAction;
                return this;
            }

            #endregion
        }

        private class MetadataContextKeyInternal<T> : MetadataContextKey, IMetadataContextKey<T>
        {
            #region Constructors

            protected internal MetadataContextKeyInternal(string key, Action<object?>? validateAction, Func<object?, ISerializationContext, bool>? canSerialize, Type? type,
                string? fieldOrPropertyName)
                : base(key, validateAction, canSerialize, type, fieldOrPropertyName)
            {
            }

            #endregion
        }

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public sealed class ContextKeyMemento : IMemento
        {
            #region Fields

            [DataMember(Name = "C")] internal MemberInfo? ConstantMember;

            [DataMember(Name = "T")] internal Type TargetTypeField;

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
                var contextKey = ConstantMember?.GetValueEx<IMetadataContextKey>(null);
                if (contextKey == null)
                    return MementoResult.Unrestored;
                return new MementoResult(contextKey, serializationContext.Metadata);
            }

            #endregion
        }

        #endregion
    }
}