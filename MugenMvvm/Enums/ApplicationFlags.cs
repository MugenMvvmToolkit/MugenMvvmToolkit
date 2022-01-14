using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class ApplicationFlags : FlagsEnumBase<ApplicationFlags, long>
    {
        public static readonly ApplicationFlags Initialized = new(1, nameof(Initialized));
        public static readonly ApplicationFlags DesignMode = new(1 << 1, nameof(DesignMode));
        public static readonly ApplicationFlags Debug = new(1 << 2, nameof(Debug));

        public ApplicationFlags(long value, string? name = null, long? flag = null, bool register = true) : base(value, name, flag, register)
        {
        }

        [Preserve(Conditional = true)]
        protected ApplicationFlags()
        {
        }
    }
}