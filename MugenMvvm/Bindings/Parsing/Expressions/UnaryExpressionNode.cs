using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class UnaryExpressionNode : ExpressionNodeBase, IUnaryExpressionNode
    {
        #region Fields

        public static readonly UnaryExpressionNode ActionMacros = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, MemberExpressionNode.Action);
        public static readonly UnaryExpressionNode EventArgsMacros = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, MemberExpressionNode.EventArgs);
        public static readonly UnaryExpressionNode TargetMacros = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, MemberExpressionNode.Self);
        public static readonly UnaryExpressionNode SourceMacros = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, MemberExpressionNode.Source);
        public static readonly UnaryExpressionNode ContextMacros = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, MemberExpressionNode.Context);
        public static readonly UnaryExpressionNode BindingMacros = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, MemberExpressionNode.Binding);

        #endregion

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

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Unary;

        public UnaryTokenType Token { get; }

        public IExpressionNode Operand { get; }

        #endregion

        #region Methods

        public static UnaryExpressionNode Get(UnaryTokenType token, IExpressionNode operand)
        {
            if ((token == UnaryTokenType.DynamicExpression || token == UnaryTokenType.StaticExpression) && operand is IMemberExpressionNode member && member.Target == null)
            {
                switch (member.Member)
                {
                    case MacrosConstant.Target:
                    case MacrosConstant.Self:
                    case MacrosConstant.This:
                        return TargetMacros;
                    case MacrosConstant.Action:
                        return ActionMacros;
                    case MacrosConstant.EventArgs:
                        return EventArgsMacros;
                    case MacrosConstant.Source:
                        return SourceMacros;
                    case MacrosConstant.Context:
                        return ContextMacros;
                    case MacrosConstant.Binding:
                        return BindingMacros;
                }
            }

            return new UnaryExpressionNode(token, operand);
        }

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            var operand = VisitWithCheck(visitor, Operand, true, ref changed, metadata);
            if (changed)
                return new UnaryExpressionNode(Token, operand);
            return this;
        }

        public override string ToString() => $"{Token.Value}{Operand}";

        #endregion
    }
}