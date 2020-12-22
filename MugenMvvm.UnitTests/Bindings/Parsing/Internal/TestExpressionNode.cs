using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestExpressionNode : ExpressionNodeBase
    {
        #region Constructors

        public TestExpressionNode(IDictionary<string, object?>? metadata = null) : base(metadata)
        {
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Parameter;

        public Func<IExpressionVisitor, IReadOnlyMetadataContext?, IExpressionNode?>? VisitHandler { get; set; }

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => VisitHandler?.Invoke(visitor, metadata) ?? this;

        #endregion
    }
}