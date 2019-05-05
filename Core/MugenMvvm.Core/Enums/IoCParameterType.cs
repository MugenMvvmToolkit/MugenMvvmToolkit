using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class IocParameterType : EnumBase<IocParameterType, int>
    {
        #region Fields

        public static readonly IocParameterType Constructor = new IocParameterType(1);
        public static readonly IocParameterType Property = new IocParameterType(2);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected IocParameterType()
        {
        }

        public IocParameterType(int value) : base(value)
        {
        }

        #endregion
    }
}