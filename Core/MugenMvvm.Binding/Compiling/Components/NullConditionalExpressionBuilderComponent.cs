using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class NullConditionalExpressionBuilderComponent : ExpressionCompilerComponent.IExpressionBuilder, IHasPriority
    {
        #region Fields

        private static readonly Type[] GenericTypeBuffer = new Type[1];
        private static readonly ParameterExpression[] ParameterExpressionBuffer = new ParameterExpression[1];
        private static readonly Expression[] ExpressionBuffer = new Expression[2];

        #endregion

        #region Properties

        public int Priority { get; set; } = 1; //todo review priority

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            if (!(expression is IHasTargetExpressionNode<IExpressionNode> hasTarget) || !(hasTarget.Target is NullConditionalMemberExpressionNode nullConditional))
                return null;

            if (context.TryGetExpression(nullConditional) != null)
                return null;

            var targetEx = context.Build(nullConditional.Target!);
            try
            {
                if (targetEx.Type.IsValueTypeUnified() && !targetEx.Type.IsNullableType())
                {
                    context.SetExpression(nullConditional, targetEx);
                    return context.Build(expression);
                }

                var variable = Expression.Variable(targetEx.Type);
                context.SetExpression(nullConditional, variable);
                var exp = context.Build(expression);
                if (exp == null)
                    return null;

                var type = exp.Type;
                if (type.IsValueTypeUnified() && !type.IsNullableType())
                {
                    GenericTypeBuffer[0] = type;
                    type = typeof(Nullable<>).MakeGenericType(GenericTypeBuffer);
                }

                var conditionalExpression = Expression.Condition(Expression.ReferenceEqual(variable, MugenExtensions.NullConstantExpression), Expression.Constant(null, type),
                    exp.ConvertIfNeed(type, false));
                ParameterExpressionBuffer[0] = variable;
                ExpressionBuffer[0] = Expression.Assign(variable, targetEx);
                ExpressionBuffer[1] = conditionalExpression;

                return Expression.Block(ParameterExpressionBuffer, ExpressionBuffer);
            }
            finally
            {
                context.ClearExpression(nullConditional);
                Array.Clear(ExpressionBuffer, 0, ExpressionBuffer.Length);
            }
        }

        #endregion
    }
}