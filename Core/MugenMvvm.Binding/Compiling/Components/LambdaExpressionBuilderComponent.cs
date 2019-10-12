using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class LambdaExpressionBuilderComponent : ExpressionCompilerComponent.IExpressionBuilder, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            if (!(expression is ILambdaExpressionNode lambdaExpression))
                return null;

            var method = context.TryGetLambdaParameter()?.ParameterType.GetMethodUnified(nameof(Action.Invoke), MemberFlags.InstancePublic);
            if (method == null)
                return null;

            var parameters = method.GetParameters();
            if (lambdaExpression.Parameters.Count != parameters.Length)
                return null;

            var lambdaParameters = new ParameterExpression[parameters.Length];
            try
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterExp = lambdaExpression.Parameters[i];
                    if (parameterExp.Type != null && !parameterExp.Type.IsAssignableFromUnified(parameters[i].ParameterType))
                        return null;

                    var parameter = Expression.Parameter(parameters[i].ParameterType, lambdaExpression.Parameters[i].Name);
                    lambdaParameters[i] = parameter;
                    context.SetParameterExpression(lambdaExpression.Parameters[i], parameter);
                }

                return Expression.Lambda(context.Build(lambdaExpression.Body), lambdaParameters);
            }
            finally
            {
                for (var i = 0; i < lambdaParameters.Length; i++)
                    context.ClearParameterExpression(lambdaExpression.Parameters[i]);
            }
        }

        #endregion
    }
}