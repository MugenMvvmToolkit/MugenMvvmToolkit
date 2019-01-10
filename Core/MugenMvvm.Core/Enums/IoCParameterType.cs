using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable, DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class IoCParameterType : EnumBase<IoCParameterType, int>
    {
        #region Fields

        public static readonly IoCParameterType Constructor = new IoCParameterType(1);
        public static readonly IoCParameterType Property = new IoCParameterType(2);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        internal IoCParameterType()
        {
        }

        public IoCParameterType(int value) : base(value)
        {
        }

        #endregion
    }
}