using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class SerializationFormat<TRequest, TResult> : EnumBase<SerializationFormat<TRequest, TResult>, string>, ISerializationFormat<TRequest, TResult>
    {
        static SerializationFormat()
        {
            ThrowOnDuplicate = false;
        }

        public SerializationFormat(string value, string? name = null, bool register = true) : base(value, name, register)
        {
        }

        [Preserve(Conditional = true)]
        protected SerializationFormat()
        {
        }

        public bool IsSerialization => true;
    }
}