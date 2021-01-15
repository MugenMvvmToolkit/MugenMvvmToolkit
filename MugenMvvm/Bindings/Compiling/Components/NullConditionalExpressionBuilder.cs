using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class NullConditionalExpressionBuilder : IExpressionBuilderComponent, IHasPriority
    {
        private static readonly Type[] GenericTypeBuffer = new Type[1];
        private static readonly ParameterExpression[] ParameterExpressionBuffer = new ParameterExpression[1];
        private static readonly Expression[] ExpressionBuffer = new Expression[2];

        private readonly HashSet<IExpressionNode> _handledExpressions;

        public NullConditionalExpressionBuilder()
        {
            _handledExpressions = new HashSet<IExpressionNode>();
        }

        public int Priority { get; set; } = CompilingComponentPriority.NullConditionalMember;

        private static bool HasNullCondition(IHasTargetExpressionNode<IExpressionNode>? target, [NotNullWhen(true)] out NullConditionalMemberExpressionNode? result)
        {
            while (target != null)
            {
                if (target is NullConditionalMemberExpressionNode r)
                {
                    result = r;
                    return true;
                }

                target = target.Target as IHasTargetExpressionNode<IExpressionNode>;
            }

            result = null;
            return false;
        }

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (expression is not IHasTargetExpressionNode<IExpressionNode> hasTarget || !HasNullCondition(hasTarget, out var nullConditional) ||
                !_handledExpressions.Add(nullConditional))
                return null;

            try
            {
                var target = context.Build(nullConditional.Target!);
                var variable = Expression.Variable(target.Type);
                context.SetExpression(nullConditional, variable);
                var exp = context.Build(expression);
                var type = exp.Type;
                if (type.IsValueType && !type.IsNullableType())
                {
                    GenericTypeBuffer[0] = type;
                    type = typeof(Nullable<>).MakeGenericType(GenericTypeBuffer);
                }

                Expression resultExpression;
                if (target.Type.IsValueType && !target.Type.IsNullableType())
                    resultExpression = exp;
                else
                {
                    var nullConstant = Expression.Constant(null, type);
                    resultExpression = Expression.Condition(target.Type.IsValueType
                        ? Expression.Equal(variable, MugenExtensions.NullConstantExpression)
                        : Expression.ReferenceEqual(variable, MugenExtensions.NullConstantExpression), nullConstant, exp.ConvertIfNeed(type, false));
                }

                ParameterExpressionBuffer[0] = variable;
                ExpressionBuffer[0] = Expression.Assign(variable, target);
                ExpressionBuffer[1] = resultExpression;

                return Expression.Block(ParameterExpressionBuffer, ExpressionBuffer);
            }
            finally
            {
                context.ClearExpression(nullConditional.Target!);
                _handledExpressions.Remove(nullConditional);
            }
        }
    }
}