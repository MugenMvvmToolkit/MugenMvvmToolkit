using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class MessengerResult : EnumBase<MessengerResult, int>
    {
        public static readonly MessengerResult Handled = new(1);
        public static readonly MessengerResult Ignored = new(2);
        public static readonly MessengerResult Invalid = new(3);

        public MessengerResult(int value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected MessengerResult()
        {
        }
    }
}