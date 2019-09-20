using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;

namespace MugenMvvm.Binding.Parsing.Nodes
{
    public sealed class LambdaExpressionNode : ExpressionNodeBase, ILambdaExpressionNode
    {
        #region Constructors

        public LambdaExpressionNode(IExpressionNode body, IReadOnlyList<IParameterExpression>? parameters)
        {
            Should.NotBeNull(body, nameof(body));
            Body = body;
            Parameters = parameters ?? Default.EmptyArray<IParameterExpression>();
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Lambda;

        public IReadOnlyList<IParameterExpression> Parameters { get; }

        public IExpressionNode Body { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            var changed = false;
            var body = VisitWithCheck(visitor, Body, true, ref changed);

            var itemsChanged = false;
            IParameterExpression[]? newParams = null;
            for (var i = 0; i < Parameters.Count; i++)
            {
                var node = VisitWithCheck(visitor, Parameters[i], true, ref itemsChanged);
                if (itemsChanged)
                    newParams = Parameters.ToArray();
                if (newParams != null)
                    newParams[i] = node;
            }

            if (changed || itemsChanged)
                return new LambdaExpressionNode(body, newParams ?? Parameters);
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