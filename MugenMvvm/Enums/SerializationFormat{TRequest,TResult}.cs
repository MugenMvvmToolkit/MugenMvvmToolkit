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
        public SerializationFormat(int value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected SerializationFormat()
        {
        }

        public bool IsSerialization => true;
    }
}