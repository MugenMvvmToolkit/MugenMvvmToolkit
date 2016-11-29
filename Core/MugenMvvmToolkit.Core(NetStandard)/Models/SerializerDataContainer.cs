using System;
using System.Runtime.Serialization;

namespace MugenMvvmToolkit.Models
{
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true, Name = "sdc")]
    [Serializable]
    public sealed class SerializerDataContainer
    {
        #region Properties

        [DataMember(Name = "d")]
        public object Data { get; set; }

        #endregion
    }
}