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
        #region Fields

        public new static readonly MetadataOperationType Get = new(1);
        public static readonly MetadataOperationType Set = new(2);
        public static readonly MetadataOperationType Remove = new(3);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected MetadataOperationType()
        {
        }

        public MetadataOperationType(int value) : base(value)
        {
        }

        #endregion
    }
}