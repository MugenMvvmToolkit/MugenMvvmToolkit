using System;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    public class TestExpressionVisitor : IExpressionVisitor
    {
        #region Properties

        public bool IsPostOrder { get; set; }

        public Func<IExpressionNode, IReadOnlyMetadataContext?, IExpressionNode?>? Visit { get; set; }

        #endregion

        #region Implementation of interfaces

        IExpressionNode? IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return Visit?.Invoke(expression, metadata);
        }

        #endregion
    }
}