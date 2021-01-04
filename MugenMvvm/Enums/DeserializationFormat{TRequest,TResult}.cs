using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class DeserializationFormat<TRequest, TResult> : EnumBase<DeserializationFormat<TRequest, TResult>, int>, IDeserializationFormat<TRequest, TResult>
    {
        #region Constructors

        [Preserve(Conditional = true)]
        protected DeserializationFormat()
        {
        }

        public DeserializationFormat(int value, string? name = null) : base(value, name)
        {
        }

        #endregion

        #region Properties

        public bool IsSerialization => false;

        #endregion
    }
}