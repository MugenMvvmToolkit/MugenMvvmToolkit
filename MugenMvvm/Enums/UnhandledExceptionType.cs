using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class UnhandledExceptionType : EnumBase<UnhandledExceptionType, int>
    {
        public static readonly UnhandledExceptionType Binding = new(1);
        public static readonly UnhandledExceptionType System = new(2);
        public static readonly UnhandledExceptionType Validation = new(3);
        public static readonly UnhandledExceptionType Command = new(4);

        public UnhandledExceptionType(int value, string? name = null, bool register = true) : base(value, name, register)
        {
        }

        [Preserve(Conditional = true)]
        protected UnhandledExceptionType()
        {
        }
    }
}