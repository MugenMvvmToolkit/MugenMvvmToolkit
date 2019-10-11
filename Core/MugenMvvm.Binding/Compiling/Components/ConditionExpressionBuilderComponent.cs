using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class ConditionExpressionBuilderComponent : ExpressionCompilerComponent.IExpressionBuilder, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            if (!(expression is IConditionExpressionNode condition))
                return null;

            var ifTrue = context.Build(condition.IfTrue);
            var ifFalse = context.Build(condition.IfFalse);
            BindingMugenExtensions.Convert(ref ifTrue, ref ifFalse, true);
            return Expression.Condition(context.Build(condition.Condition).ConvertIfNeed(typeof(bool), true), ifTrue, ifFalse);
        }

        #endregion
    }
}