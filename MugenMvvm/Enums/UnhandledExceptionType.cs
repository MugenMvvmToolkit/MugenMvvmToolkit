using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class UnhandledExceptionType : EnumBase<UnhandledExceptionType, int>
    {
        public static readonly UnhandledExceptionType Binding = new(1);
        public static readonly UnhandledExceptionType System = new(2);
        public static readonly UnhandledExceptionType Validation = new(3);

        public UnhandledExceptionType(int value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected UnhandledExceptionType()
        {
        }
    }
}