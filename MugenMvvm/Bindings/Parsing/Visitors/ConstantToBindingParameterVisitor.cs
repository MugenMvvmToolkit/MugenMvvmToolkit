using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Visitors
{
    public sealed class ConstantToBindingParameterVisitor : IExpressionVisitor
    {
        public ExpressionTraversalType TraversalType => ExpressionTraversalType.Postorder;

        public IExpressionNode Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata) =>
            expression is IConstantExpressionNode constant && constant.Value is not Type
                ? new BindingInstanceMemberExpressionNode(constant.Value == null && constant.Type != typeof(object) ? constant : constant.Value, "", -1, default,
                    MemberFlags.Static, null, expression, expression.Metadata)
                : expression;
    }
}