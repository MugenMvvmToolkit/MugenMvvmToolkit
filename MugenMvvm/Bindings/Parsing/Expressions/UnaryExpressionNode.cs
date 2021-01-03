using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class UnaryExpressionNode : ExpressionNodeBase<IUnaryExpressionNode>, IUnaryExpressionNode
    {
        #region Fields

        public static readonly UnaryExpressionNode ActionMacros = new(UnaryTokenType.DynamicExpression, MemberExpressionNode.Action);
        public static readonly UnaryExpressionNode EventArgsMacros = new(UnaryTokenType.DynamicExpression, MemberExpressionNode.EventArgs);
        public static readonly UnaryExpressionNode TargetMacros = new(UnaryTokenType.DynamicExpression, MemberExpressionNode.Self);
        public static readonly UnaryExpressionNode SourceMacros = new(UnaryTokenType.DynamicExpression, MemberExpressionNode.Source);
        public static readonly UnaryExpressionNode ContextMacros = new(UnaryTokenType.DynamicExpression, MemberExpressionNode.Context);
        public static readonly UnaryExpressionNode BindingMacros = new(UnaryTokenType.DynamicExpression, MemberExpressionNode.Binding);

        #endregion

        #region Constructors

        public UnaryExpressionNode(UnaryTokenType token, IExpressionNode operand, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
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
                return new UnaryExpressionNode(Token, operand, Metadata);
            return this;
        }

        protected override IUnaryExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new UnaryExpressionNode(Token, Operand, metadata);

        protected override bool Equals(IUnaryExpressionNode other, IExpressionEqualityComparer? comparer) => Token == other.Token && Operand.Equals(other.Operand, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => HashCode.Combine(hashCode, Token.GetHashCode(), Operand.GetHashCode(comparer));

        public override string ToString() => $"{Token.Value}{Operand}";

        #endregion
    }
}