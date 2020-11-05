using System;
using System.Runtime.Serialization;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class MetadataOperationType : EnumBase<MetadataOperationType, int>
    {
        #region Fields

        public new static readonly MetadataOperationType Get = new MetadataOperationType(1);
        public static readonly MetadataOperationType Set = new MetadataOperationType(2);
        public static readonly MetadataOperationType Remove = new MetadataOperationType(3);

        #endregion

        #region Constructors

        public MetadataOperationType(int value) : base(value)
        {
        }

        #endregion
    }
}