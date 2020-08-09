using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class NullConditionalExpressionBuilder : IExpressionBuilderComponent, IHasPriority
    {
        #region Fields

        private readonly HashSet<IExpressionNode> _handledExpressions;

        private static readonly Type[] GenericTypeBuffer = new Type[1];
        private static readonly ParameterExpression[] ParameterExpressionBuffer = new ParameterExpression[1];
        private static readonly Expression[] ExpressionBuffer = new Expression[2];

        #endregion

        #region Constructors

        public NullConditionalExpressionBuilder()
        {
            _handledExpressions = new HashSet<IExpressionNode>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = CompilingComponentPriority.NullConditionalMember;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (!(expression is IHasTargetExpressionNode<IExpressionNode> hasTarget) || !HasNullCondition(hasTarget, out var nullConditional) || !_handledExpressions.Add(nullConditional))
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

        #endregion

        #region Methods

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

        #endregion
    }
}