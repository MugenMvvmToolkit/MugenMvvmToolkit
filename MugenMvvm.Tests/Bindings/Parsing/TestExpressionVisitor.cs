using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Bindings.Parsing
{
    public class TestExpressionVisitor : IExpressionVisitor
    {
        public Func<IExpressionNode, IReadOnlyMetadataContext?, IExpressionNode?>? Visit { get; set; }

        public ExpressionTraversalType TraversalType { get; set; } = ExpressionTraversalType.Preorder;

        IExpressionNode? IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata) => Visit?.Invoke(expression, metadata);
    }
}