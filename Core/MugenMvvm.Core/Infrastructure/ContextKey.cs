using System;
using System.Reflection;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure
{
    public class ContextKey : IContextKey
    {
        #region Fields

        private readonly Func<object?, ISerializationContext, bool>? _canSerialize;
        private readonly string? _fieldOrPropertyName;
        private readonly Type? _type;
        private readonly Action<object?>? _validateAction;

        #endregion

        #region Constructors

        protected ContextKey(string key, Action<object?>? validateAction, Func<object?, ISerializationContext, bool>? canSerialize, Type? type, string? fieldOrPropertyName)
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

        public IMemento? GetMemento()
        {
            if (_type != null && !string.IsNullOrEmpty(_fieldOrPropertyName))
                return new ContextKeyMemento(this);
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

        #endregion

        #region Nested types

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public sealed class ContextKeyMemento : IMemento
        {
            #region Fields

            [DataMember(Name = "T")]
            internal Type TargetTypeField;

            [DataMember(Name = "D")]
            internal Type DeclaredType;

            [DataMember(Name = "F")]
            internal string FieldOrPropertyName;

            #endregion

            #region Constructors

            internal ContextKeyMemento()
            {
            }

            internal ContextKeyMemento(ContextKey key)
            {
                TargetTypeField = key.GetType();
                DeclaredType = key._type!;
                FieldOrPropertyName = key._fieldOrPropertyName!;
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
                Should.NotBeNull(DeclaredType, nameof(DeclaredType));
                Should.NotBeNull(FieldOrPropertyName, nameof(FieldOrPropertyName));
                MemberInfo member = DeclaredType.GetFieldUnified(FieldOrPropertyName, MemberFlags.StaticOnly);
                if (member == null)
                    member = DeclaredType.GetPropertyUnified(FieldOrPropertyName, MemberFlags.StaticOnly);
                var contextKey = member?.GetValueEx<IContextKey>(null);
                if (contextKey == null)
                    return MementoResult.Unrestored;
                return new MementoResult(contextKey, serializationContext.Metadata);
            }

            #endregion
        }

        #endregion
    }

    public class ContextKey<T> : ContextKey, IContextKey<T>
    {
        #region Constructors

        protected internal ContextKey(string key, Action<object?>? validateAction, Func<object?, ISerializationContext, bool>? canSerialize, Type? type, string? fieldOrPropertyName)
            : base(key, validateAction, canSerialize, type, fieldOrPropertyName)
        {
        }

        #endregion
    }
}