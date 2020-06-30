using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.UnitTest.Binding.Compiling.Internal
{
    public class TestExpressionBuilderContext : MetadataOwnerBase, IExpressionBuilderContext
    {
        #region Fields

        private readonly Dictionary<IExpressionNode, Expression> _dictionary;

        #endregion

        #region Constructors

        public TestExpressionBuilderContext(IReadOnlyMetadataContext? metadata = null, IMetadataContextManager? metadataContextManager = null)
            : base(metadata, metadataContextManager)
        {
            MetadataExpression = Expression.Parameter(typeof(IReadOnlyMetadataContext));
            _dictionary = new Dictionary<IExpressionNode, Expression>();
        }

        #endregion

        #region Properties

        public Expression MetadataExpression { get; set; }

        public Func<IExpressionNode, Expression?>? TryGetExpression { get; set; }

        public Action<IExpressionNode, Expression>? SetExpression { get; set; }

        public Action<IExpressionNode>? ClearExpression { get; set; }

        public Func<IExpressionNode, Expression?>? Build { get; set; } = node => Expression.Constant(((IConstantExpressionNode)node).Value, ((IConstantExpressionNode)node).Type);

        #endregion

        #region Implementation of interfaces

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

        #endregion
    }
}