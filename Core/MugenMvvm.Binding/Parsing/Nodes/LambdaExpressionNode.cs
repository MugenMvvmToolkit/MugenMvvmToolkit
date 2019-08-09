using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;

namespace MugenMvvm.Binding.Parsing.Nodes
{
    public sealed class LambdaExpressionNode : ExpressionNodeBase, ILambdaExpressionNode
    {
        #region Constructors

        public LambdaExpressionNode(IExpressionNode body, IReadOnlyList<string> parameters)
        {
            Should.NotBeNull(body, nameof(body));
            Should.NotBeNull(parameters, nameof(parameters));
            Body = body;
            Parameters = parameters;
            BindingMugenExtensions.CheckDuplicateLambdaParameter(Parameters);
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Lambda;

        public IReadOnlyList<string> Parameters { get; }

        public IExpressionNode Body { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            var changed = false;
            var body = VisitWithCheck(visitor, Body, true, ref changed);
            if (changed)
                return new LambdaExpressionNode(body, Parameters);
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