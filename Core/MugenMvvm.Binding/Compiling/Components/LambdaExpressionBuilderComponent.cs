using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class LambdaExpressionBuilderComponent : IExpressionBuilderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = CompilingComponentPriority.Lambda;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (!(expression is ILambdaExpressionNode lambdaExpression))
                return null;

            var method = context.TryGetLambdaParameter()?.ParameterType.GetMethod(nameof(Action.Invoke), BindingFlagsEx.InstancePublic);
            if (method == null)
            {
                context.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileLambdaExpressionDelegateFormat1.Format(lambdaExpression));
                return null;
            }

            var parameters = method.GetParameters();
            if (lambdaExpression.Parameters.Count != parameters.Length)
            {
                context.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileLambdaExpressionParameterCountFormat2.Format(lambdaExpression, method));
                return null;
            }

            var lambdaParameters = new ParameterExpression[parameters.Length];
            try
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterExp = lambdaExpression.Parameters[i];
                    if (parameterExp.Type != null && !parameterExp.Type.IsAssignableFrom(parameters[i].ParameterType))
                    {
                        context.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileLambdaExpressionParameterNotAssignableFormat3.Format(lambdaExpression, parameterExp, parameters[i]));
                        return null;
                    }

                    var parameter = Expression.Parameter(parameters[i].ParameterType, lambdaExpression.Parameters[i].Name);
                    lambdaParameters[i] = parameter;
                    context.SetExpression(parameterExp, parameter);
                }

                return Expression.Lambda(context.Build(lambdaExpression.Body), lambdaParameters);
            }
            finally
            {
                for (var i = 0; i < lambdaParameters.Length; i++)
                    context.ClearExpression(lambdaExpression.Parameters[i]);
            }
        }

        #endregion
    }
}