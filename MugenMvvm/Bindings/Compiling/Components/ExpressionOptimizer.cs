using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Extensions;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class ExpressionOptimizer : ComponentDecoratorBase<IExpressionCompiler, IExpressionBuilderComponent>, IExpressionBuilderComponent //todo test
    {
        private readonly Visitor _visitor;
        private bool _building;

        public ExpressionOptimizer(int priority = ParsingComponentPriority.Optimizer) : base(priority)
        {
            _visitor = new Visitor();
        }

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (_building)
                return Components.TryBuild(context, expression);

            _building = true;
            Expression? result;
            try
            {
                result = Components.TryBuild(context, expression);
            }
            finally
            {
                _building = false;
            }

            if (result == null)
                return null;
            return _visitor.Visit(result);
        }

        private sealed class Visitor : ExpressionVisitor
        {
            private bool _hasParameter;
            private bool _visiting;

            private static bool IsItemOrList(Type type) => type.IsGenericType && MugenExtensions.RawMethodMapping.ContainsKey(type.GetGenericTypeDefinition());

            private static bool IsValid(ExpressionType type) =>
                type != ExpressionType.Parameter && type != ExpressionType.Constant && type != ExpressionType.Convert && type != ExpressionType.ConvertChecked &&
                type != ExpressionType.Lambda;

            public override Expression? Visit(Expression? node)
            {
                if (node != null && !_visiting && IsValid(node.NodeType) && !IsItemOrList(node.Type))
                {
                    _hasParameter = false;
                    _visiting = true;
                    try
                    {
                        Visit(node);
                    }
                    finally
                    {
                        _visiting = false;
                    }

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
        }
    }
}