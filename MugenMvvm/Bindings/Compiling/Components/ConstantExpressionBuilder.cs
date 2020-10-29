using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class ConstantExpressionBuilder : IExpressionBuilderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = CompilingComponentPriority.Constant;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (expression is ConstantExpressionNode constantExpression)
                return constantExpression.ConstantExpression ?? Expression.Constant(constantExpression.Value, constantExpression.Type);
            if (expression is IConstantExpressionNode constant)
                return Expression.Constant(constant.Value, constant.Type);
            return null;
        }

        #endregion
    }
}