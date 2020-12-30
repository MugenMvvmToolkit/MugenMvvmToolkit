using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class BindingState : EnumBase<BindingState, int>
    {
        #region Fields

        public static readonly BindingState Valid = new(1);
        public static readonly BindingState Disposed = new(2);
        public static readonly BindingState Invalid = new(3);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected BindingState()
        {
        }

        public BindingState(int value)
            : base(value)
        {
        }

        #endregion
    }
}