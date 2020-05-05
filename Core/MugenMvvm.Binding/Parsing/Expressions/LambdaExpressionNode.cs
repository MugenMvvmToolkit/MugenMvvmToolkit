using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class LambdaExpressionNode : ExpressionNodeBase, ILambdaExpressionNode
    {
        #region Constructors

        public LambdaExpressionNode(IExpressionNode body, IReadOnlyList<IParameterExpressionNode>? parameters)
        {
            Should.NotBeNull(body, nameof(body));
            Body = body;
            Parameters = parameters ?? Default.EmptyArray<IParameterExpressionNode>();
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Lambda;

        public IReadOnlyList<IParameterExpressionNode> Parameters { get; }

        public IExpressionNode Body { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            var body = VisitWithCheck(visitor, Body, true, ref changed, metadata);
            var newParams = VisitWithCheck(visitor, Parameters, ref changed, metadata);
            if (changed)
                return new LambdaExpressionNode(body, newParams);
            return this;
        }

        public override string ToString()
        {
            if (Parameters.Count == 0)
                return "() => " + Body;
            return $"({string.Join(", ", Parameters)}) => {Body}";
        }

        #endregion
    }
}