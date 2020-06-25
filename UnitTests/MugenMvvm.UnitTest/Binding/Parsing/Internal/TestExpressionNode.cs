using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    public class TestExpressionNode : ExpressionNodeBase
    {
        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Parameter;

        public Func<IExpressionVisitor, IReadOnlyMetadataContext?, IExpressionNode?>? Visit { get; set; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            return Visit?.Invoke(visitor, metadata) ?? this;
        }

        #endregion
    }
}