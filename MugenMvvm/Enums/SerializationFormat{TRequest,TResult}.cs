using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class SerializationFormat<TRequest, TResult> : EnumBase<SerializationFormat<TRequest, TResult>, int>, ISerializationFormat<TRequest, TResult>
    {
        #region Constructors

        [Preserve(Conditional = true)]
        protected SerializationFormat()
        {
        }

        public SerializationFormat(int value, string? name) : base(value, name)
        {
        }

        #endregion

        #region Properties

        public bool IsSerialization => true;

        #endregion
    }
}