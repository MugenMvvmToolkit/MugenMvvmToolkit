using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;

namespace MugenMvvm.Binding.Parsing.Nodes
{
    public sealed class BinaryExpressionNode : ExpressionNodeBase, IBinaryExpressionNode
    {
        #region Constructors

        public BinaryExpressionNode(BinaryTokenType token, IExpressionNode left, IExpressionNode right)
        {
            Should.NotBeNull(token, nameof(token));
            Should.NotBeNull(left, nameof(left));
            Should.NotBeNull(right, nameof(right));
            Left = left;
            Right = right;
            Token = token;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Binary;

        public IExpressionNode Left { get; }

        public IExpressionNode Right { get; }

        public BinaryTokenType Token { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            bool changed = false;
            var left = VisitWithCheck(visitor, Left, true, ref changed);
            var right = VisitWithCheck(visitor, Right, true, ref changed);
            if (changed)
                return new BinaryExpressionNode(Token, left, right);
            return this;
        }

        public override string ToString()
        {
            return $"({Left} {Token.Value} {Right})";
        }

        #endregion
    }
}