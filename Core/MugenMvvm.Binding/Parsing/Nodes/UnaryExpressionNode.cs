using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;

namespace MugenMvvm.Binding.Parsing.Nodes
{
    public sealed class UnaryExpressionNode : ExpressionNodeBase, IUnaryExpressionNode
    {
        #region Constructors

        public UnaryExpressionNode(UnaryTokenType token, IExpressionNode operand)
        {
            Should.NotBeNull(token, nameof(token));
            Should.NotBeNull(operand, nameof(operand));
            Token = token;
            Operand = operand;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Unary;

        public UnaryTokenType Token { get; }

        public IExpressionNode Operand { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            var changed = false;
            var operand = VisitWithCheck(visitor, Operand, true, ref changed);
            if (changed)
                return new UnaryExpressionNode(Token, operand);
            return this;
        }

        public override string ToString()
        {
            return $"{Token.Value}{Operand}";
        }

        #endregion
    }
}