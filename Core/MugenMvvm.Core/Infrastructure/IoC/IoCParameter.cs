using System;
using System.Runtime.Serialization;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.IoC;

namespace MugenMvvm.Infrastructure.IoC
{
    [Serializable, DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class IoCParameter : IIoCParameter
    {
        #region Constructors

        public IoCParameter(string name, object value, IoCParameterType parameterType)
        {
            Name = name;
            Value = value;
            ParameterType = parameterType;
        }

        #endregion

        #region Properties

        [DataMember(Name = "p")]
        public IoCParameterType ParameterType { get; internal set; }

        [DataMember(Name = "n")]
        public string Name { get; internal set; }

        [DataMember(Name = "v")]
        public object Value { get; internal set; }

        #endregion
    }
}