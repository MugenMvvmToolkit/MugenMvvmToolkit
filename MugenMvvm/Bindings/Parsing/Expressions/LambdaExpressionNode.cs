using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class LambdaExpressionNode : ExpressionNodeBase, ILambdaExpressionNode
    {
        #region Constructors

        public LambdaExpressionNode(IExpressionNode body, IReadOnlyList<IParameterExpressionNode>? parameters, IDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(body, nameof(body));
            Body = body;
            Parameters = parameters ?? Default.Array<IParameterExpressionNode>();
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Lambda;

        public IReadOnlyList<IParameterExpressionNode> Parameters { get; }

        public IExpressionNode Body { get; }

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            var body = VisitWithCheck(visitor, Body, true, ref changed, metadata);
            var newParams = VisitWithCheck(visitor, Parameters, ref changed, metadata);
            if (changed)
                return new LambdaExpressionNode(body, newParams, MetadataRaw);
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