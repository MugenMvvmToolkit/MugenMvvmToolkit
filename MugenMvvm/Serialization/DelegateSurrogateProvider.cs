using System;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Serialization
{
    public sealed class DelegateSurrogateProvider<TFrom, TSurrogate> : ISurrogateProvider
        where TFrom : class
        where TSurrogate : class
    {
        #region Fields

        private readonly Func<TSurrogate?, ISerializationContext, TFrom?> _getDeserializedObject;
        private readonly Func<TFrom?, ISerializationContext, TSurrogate?> _getObjectToSerialize;

        #endregion

        #region Constructors

        public DelegateSurrogateProvider(Func<TFrom?, ISerializationContext, TSurrogate?> getObjectToSerialize, Func<TSurrogate?, ISerializationContext, TFrom?> getDeserializedObject)
        {
            Should.NotBeNull(getObjectToSerialize, nameof(getObjectToSerialize));
            Should.NotBeNull(getDeserializedObject, nameof(getDeserializedObject));
            _getObjectToSerialize = getObjectToSerialize;
            _getDeserializedObject = getDeserializedObject;
        }

        #endregion

        #region Properties

        public Type FromType => typeof(TFrom);

        public Type SurrogateType => typeof(TSurrogate);

        #endregion

        #region Implementation of interfaces

        public object? GetObjectToSerialize(object? instance, ISerializationContext serializationContext) => _getObjectToSerialize((TFrom?) instance, serializationContext);

        public object? GetDeserializedObject(object? surrogate, ISerializationContext serializationContext) => _getDeserializedObject((TSurrogate?) surrogate, serializationContext);

        #endregion

        #region Methods

        public static DelegateSurrogateProvider<TFrom, TSurrogate> Get(Func<TFrom?, ISerializationContext, TSurrogate?> getObjectToSerialize, Func<TSurrogate?, ISerializationContext, TFrom?> getDeserializedObject)
            => new(getObjectToSerialize, getDeserializedObject);

        #endregion
    }
}