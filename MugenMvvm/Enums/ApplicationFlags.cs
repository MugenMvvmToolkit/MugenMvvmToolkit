using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class ApplicationFlags : FlagsEnumBase<ApplicationFlags, long>
    {
        public static readonly ApplicationFlags Initialized = new(1, nameof(Initialized));
        public static readonly ApplicationFlags DesignMode = new(1 << 1, nameof(DesignMode));

        [Preserve(Conditional = true)]
        public ApplicationFlags()
        {
        }

        public ApplicationFlags(long value, string? name = null, long? flag = null) : base(value, name, flag)
        {
        }
    }
}