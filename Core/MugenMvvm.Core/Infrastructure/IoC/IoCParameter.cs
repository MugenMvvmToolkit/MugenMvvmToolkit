using System;
using System.Runtime.Serialization;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.IoC;

namespace MugenMvvm.Infrastructure.IoC
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class IocParameter : IIocParameter
    {
        #region Constructors

        public IocParameter(string name, object value, IocParameterType parameterType)
        {
            Name = name;
            Value = value;
            ParameterType = parameterType;
        }

        #endregion

        #region Properties

        [DataMember(Name = "p")]
        public IocParameterType ParameterType { get; internal set; }

        [DataMember(Name = "n")]
        public string Name { get; internal set; }

        [DataMember(Name = "v")]
        public object Value { get; internal set; }

        #endregion
    }
}