using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
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

        public static readonly ArgumentFlags Metadata = new(1 << 0, nameof(Metadata), -1);
        public static readonly ArgumentFlags Optional = new(1 << 4, nameof(Optional), -2);
        public static readonly ArgumentFlags ParamArray = new(1 << 1, nameof(ParamArray), -3);
        public static readonly ArgumentFlags EmptyParamArray = new((byte) (ParamArray.Value | 1 << 2), nameof(EmptyParamArray), -4);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ArgumentFlags()
        {
        }

        public ArgumentFlags(int value, string name, int priority) : base(value, name)
        {
            Priority = priority;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion
    }
}