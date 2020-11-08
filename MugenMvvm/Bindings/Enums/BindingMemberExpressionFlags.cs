using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class BindingMemberExpressionFlags : FlagsEnumBase<BindingMemberExpressionFlags, ushort>
    {
        #region Fields

        public static readonly BindingMemberExpressionFlags StablePath = new BindingMemberExpressionFlags(1 << 0);
        public static readonly BindingMemberExpressionFlags Observable = new BindingMemberExpressionFlags(1 << 1);
        public static readonly BindingMemberExpressionFlags ObservableMethods = new BindingMemberExpressionFlags(1 << 2);
        public static readonly BindingMemberExpressionFlags Optional = new BindingMemberExpressionFlags(1 << 3);
        public static readonly BindingMemberExpressionFlags Target = new BindingMemberExpressionFlags(1 << 4);
        public static readonly BindingMemberExpressionFlags DataContextPath = new BindingMemberExpressionFlags(1 << 5);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected BindingMemberExpressionFlags()
        {
        }

        public BindingMemberExpressionFlags(ushort value, string? name) : base(value, name)
        {
        }

        public BindingMemberExpressionFlags(ushort value) : base(value)
        {
        }

        #endregion
    }
}