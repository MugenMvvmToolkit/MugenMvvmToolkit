using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class MessengerResult : EnumBase<MessengerResult, int>
    {
        #region Fields

        public static readonly MessengerResult Handled = new MessengerResult(1);
        public static readonly MessengerResult Ignored = new MessengerResult(2);
        public static readonly MessengerResult Invalid = new MessengerResult(3);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected MessengerResult()
        {
        }

        public MessengerResult(int value) : base(value)
        {
        }

        #endregion
    }
}