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
    public class ArgumentFlags : FlagsEnumBase<ArgumentFlags, ushort>, IHasPriority
    {
        public static readonly ArgumentFlags Metadata = new(1 << 0, -1, nameof(Metadata));
        public static readonly ArgumentFlags Optional = new(1 << 4, -2, nameof(Optional));
        public static readonly ArgumentFlags ParamArray = new(1 << 1, -3, nameof(ParamArray));
        public static readonly ArgumentFlags EmptyParamArray = new((ushort) (ParamArray.Value | (1 << 2)), -4, nameof(EmptyParamArray));

        public ArgumentFlags(ushort value, int priority, string? name = null, long? flag = null) : base(value, name, flag)
        {
            Priority = priority;
        }

        [Preserve(Conditional = true)]
        protected ArgumentFlags()
        {
        }

        public int Priority { get; set; }
    }
}