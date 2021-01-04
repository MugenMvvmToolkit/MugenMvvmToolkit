using MugenMvvm.Attributes;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    public class ExpressionTraversalType : EnumBase<ExpressionTraversalType, int>
    {
        #region Fields

        public const int InorderValue = 1;
        public const int PreorderValue = 2;
        public const int PostorderValue = 3;

        public static readonly ExpressionTraversalType Inorder = new(InorderValue);
        public static readonly ExpressionTraversalType Preorder = new(PreorderValue);
        public static readonly ExpressionTraversalType Postorder = new(PostorderValue);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ExpressionTraversalType()
        {
        }

        public ExpressionTraversalType(int value, string? name = null) : base(value, name)
        {
        }

        #endregion
    }
}