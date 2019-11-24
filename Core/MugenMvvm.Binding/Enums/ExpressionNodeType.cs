using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
#pragma warning disable 660,661
    public class ExpressionNodeType : EnumBase<ExpressionNodeType, int>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly ExpressionNodeType Binary = new ExpressionNodeType(1);
        public static readonly ExpressionNodeType Condition = new ExpressionNodeType(2);
        public static readonly ExpressionNodeType Constant = new ExpressionNodeType(3);
        public static readonly ExpressionNodeType Index = new ExpressionNodeType(4);
        public static readonly ExpressionNodeType Member = new ExpressionNodeType(5);
        public static readonly ExpressionNodeType MethodCall = new ExpressionNodeType(6);
        public static readonly ExpressionNodeType Unary = new ExpressionNodeType(7);
        public static readonly ExpressionNodeType Lambda = new ExpressionNodeType(8);
        public static readonly ExpressionNodeType Parameter = new ExpressionNodeType(9);
        public static readonly ExpressionNodeType BindingMember = new ExpressionNodeType(10);
        public static readonly ExpressionNodeType BindingParameter = new ExpressionNodeType(11);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ExpressionNodeType()
        {
        }

        public ExpressionNodeType(int value) : base(value)
        {
        }

        public ExpressionNodeType(int value, string name) : base(value, name)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ExpressionNodeType? left, ExpressionNodeType? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ExpressionNodeType? left, ExpressionNodeType? right)
        {
            return !(left == right);
        }

        protected override bool Equals(int value)
        {
            return Value == value;
        }

        #endregion
    }
}