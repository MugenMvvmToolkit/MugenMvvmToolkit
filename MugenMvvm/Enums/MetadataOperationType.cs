using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class MetadataOperationType : EnumBase<MetadataOperationType, int>
    {
        public new static readonly MetadataOperationType Get = new(1);
        public static readonly MetadataOperationType Set = new(2);
        public static readonly MetadataOperationType Remove = new(3);

        public MetadataOperationType(int value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected MetadataOperationType()
        {
        }
    }
}