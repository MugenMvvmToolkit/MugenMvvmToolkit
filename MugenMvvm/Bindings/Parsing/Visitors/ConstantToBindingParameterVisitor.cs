using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Visitors
{
    public sealed class ConstantToBindingParameterVisitor : IExpressionVisitor//todo review
    {
        #region Properties

        public ExpressionTraversalType TraversalType => ExpressionTraversalType.Postorder;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata) =>
            expression is IConstantExpressionNode ? new BindingInstanceMemberExpressionNode(expression, "", -1, default, MemberFlags.Static, null, expression, expression.Metadata) : expression;

        #endregion
    }
}