using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class DeserializationFormat<TRequest, TResult> : EnumBase<DeserializationFormat<TRequest, TResult>, int>, IDeserializationFormat<TRequest, TResult>
    {
        public DeserializationFormat(int value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected DeserializationFormat()
        {
        }

        public bool IsSerialization => false;
    }
}