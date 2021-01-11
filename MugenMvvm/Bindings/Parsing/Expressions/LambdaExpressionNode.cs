using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class LambdaExpressionNode : ExpressionNodeBase<ILambdaExpressionNode>, ILambdaExpressionNode
    {
        #region Fields

        private readonly object? _parameters;

        #endregion

        #region Constructors

        public LambdaExpressionNode(IExpressionNode body, ItemOrIReadOnlyList<IParameterExpressionNode> parameters, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(body, nameof(body));
            Body = body;
            _parameters = parameters.GetRawValue();
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Lambda;

        public ItemOrIReadOnlyList<IParameterExpressionNode> Parameters => ItemOrIReadOnlyList.FromRawValue<IParameterExpressionNode>(_parameters);

        public IExpressionNode Body { get; }

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            var body = VisitWithCheck(visitor, Body, true, ref changed, metadata);
            var newParams = VisitWithCheck(visitor, Parameters, ref changed, metadata);
            if (changed)
                return new LambdaExpressionNode(body, newParams, Metadata);
            return this;
        }

        protected override ILambdaExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new LambdaExpressionNode(Body, Parameters, metadata);

        protected override bool Equals(ILambdaExpressionNode other, IExpressionEqualityComparer? comparer) => Body.Equals(other.Body, comparer) && Equals(Parameters, other.Parameters, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => GetHashCode(hashCode, Body, Parameters, comparer);

        public override string ToString()
        {
            if (Parameters.Count == 0)
                return "() => " + Body;
            return $"({string.Join(", ", Parameters.AsList())}) => {Body}";
        }

        #endregion
    }
}