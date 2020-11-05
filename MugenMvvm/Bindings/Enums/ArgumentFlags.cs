using System;
using System.Runtime.Serialization;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class ArgumentFlags : FlagsEnumBase<ArgumentFlags, int>, IHasPriority
    {
        #region Fields

        public static readonly ArgumentFlags Metadata = new ArgumentFlags(1 << 0, -1);
        public static readonly ArgumentFlags Optional = new ArgumentFlags(1 << 4, -2);
        public static readonly ArgumentFlags ParamArray = new ArgumentFlags(1 << 1, -3);
        public static readonly ArgumentFlags EmptyParamArray = new ArgumentFlags((byte) (ParamArray.Value | 1 << 2), -4);

        #endregion

        #region Constructors

        public ArgumentFlags(int value, int priority) : base(value)
        {
            Priority = priority;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion
    }
}