using System;
using System.Linq.Expressions;
using MugenMvvm.Extensions;

namespace MugenMvvm.Bindings.Compiling
{
    public sealed class ExpressionOptimizer : ExpressionVisitor//todo test
    {
        #region Fields

        private Expression? _currentExpression;
        private bool _hasParameter;

        #endregion

        #region Methods

        public override Expression? Visit(Expression? node)
        {
            if (node != null && _currentExpression == null && IsValid(node.NodeType))
            {
                _hasParameter = false;
                _currentExpression = node;
                Visit(node);
                _currentExpression = null;
                if (!_hasParameter)
                    return Expression.Constant(Expression.Lambda<Func<object?>>(node.ConvertIfNeed(typeof(object), false)).CompileEx().Invoke(), node.Type);
            }

            return base.Visit(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _hasParameter = true;
            return base.VisitParameter(node);
        }

        private static bool IsValid(ExpressionType type) =>
            type != ExpressionType.Parameter && type != ExpressionType.Constant && type != ExpressionType.Convert && type != ExpressionType.ConvertChecked && type != ExpressionType.Lambda;

        #endregion
    }
}