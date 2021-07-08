using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Internal
{
    public class TestExpressionBuilderContext : MetadataOwnerBase, IExpressionBuilderContext
    {
        private readonly Dictionary<IExpressionNode, Expression> _dictionary;

        public TestExpressionBuilderContext(IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            MetadataExpression = Expression.Parameter(typeof(IReadOnlyMetadataContext));
            _dictionary = new Dictionary<IExpressionNode, Expression>();
        }

        public Func<IExpressionNode, Expression?>? TryGetExpression { get; set; }

        public Action<IExpressionNode, Expression>? SetExpression { get; set; }

        public Action<IExpressionNode>? ClearExpression { get; set; }

        public Func<IExpressionNode, Expression?>? Build { get; set; } = node => Expression.Constant(((IConstantExpressionNode)node).Value, ((IConstantExpressionNode)node).Type);

        public Expression MetadataExpression { get; set; }

        Expression? IExpressionBuilderContext.TryGetExpression(IExpressionNode expression)
        {
            var result = TryGetExpression?.Invoke(expression);
            if (result == null)
                _dictionary.TryGetValue(expression, out result);
            return result;
        }

        void IExpressionBuilderContext.SetExpression(IExpressionNode expression, Expression value)
        {
            if (SetExpression == null)
                _dictionary[expression] = value;
            else
                SetExpression.Invoke(expression, value);
        }

        void IExpressionBuilderContext.ClearExpression(IExpressionNode expression)
        {
            if (ClearExpression == null)
                _dictionary.Remove(expression);
            else
                ClearExpression.Invoke(expression);
        }

        Expression? IExpressionBuilderContext.TryBuild(IExpressionNode expression)
        {
            var result = Build?.Invoke(expression);
            if (result == null)
                _dictionary.TryGetValue(expression, out result);
            return result;
        }
    }
}