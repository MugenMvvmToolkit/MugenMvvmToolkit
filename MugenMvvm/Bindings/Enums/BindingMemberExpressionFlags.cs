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

        public static readonly BindingMemberExpressionFlags StablePath = new(1 << 0, nameof(StablePath));
        public static readonly BindingMemberExpressionFlags Observable = new(1 << 1, nameof(Observable));
        public static readonly BindingMemberExpressionFlags ObservableMethods = new(1 << 2, nameof(ObservableMethods));
        public static readonly BindingMemberExpressionFlags Optional = new(1 << 3, nameof(Optional));
        public static readonly BindingMemberExpressionFlags Target = new(1 << 4, nameof(Target));
        public static readonly BindingMemberExpressionFlags ParentDataContext = new(1 << 5, nameof(ParentDataContext));

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