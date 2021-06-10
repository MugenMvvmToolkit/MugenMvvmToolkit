using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class BindingMemberExpressionFlags : FlagsEnumBase<BindingMemberExpressionFlags, ushort>
    {
        public static readonly BindingMemberExpressionFlags StablePath = new(1 << 0, nameof(StablePath));
        public static readonly BindingMemberExpressionFlags Observable = new(1 << 1, nameof(Observable));
        public static readonly BindingMemberExpressionFlags ObservableMethods = new(1 << 2, nameof(ObservableMethods));
        public static readonly BindingMemberExpressionFlags Optional = new(1 << 3, nameof(Optional));
        public static readonly BindingMemberExpressionFlags Target = new(1 << 4, nameof(Target));
        public static readonly BindingMemberExpressionFlags ParentDataContext = new(1 << 5, nameof(ParentDataContext));

        public BindingMemberExpressionFlags(ushort value, string? name = null, long? flag = null) : base(value, name, flag)
        {
        }

        [Preserve(Conditional = true)]
        protected BindingMemberExpressionFlags()
        {
        }
    }
}