using System;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestExpressionVisitor : IExpressionVisitor
    {
        #region Properties

        public bool IsPostOrder { get; set; }

        public Func<IExpressionNode, IReadOnlyMetadataContext?, IExpressionNode?>? Visit { get; set; }

        #endregion

        #region Implementation of interfaces

        IExpressionNode? IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata) => Visit?.Invoke(expression, metadata);

        #endregion
    }
}