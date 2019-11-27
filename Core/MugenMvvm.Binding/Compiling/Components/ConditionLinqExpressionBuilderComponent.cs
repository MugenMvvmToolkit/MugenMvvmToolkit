using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class ConditionLinqExpressionBuilderComponent : ILinqExpressionBuilderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = CompilingComponentPriority.Condition;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(ILinqExpressionBuilderContext context, IExpressionNode expression)
        {
            if (!(expression is IConditionExpressionNode condition))
                return null;

            var ifTrue = context.Build(condition.IfTrue);
            var ifFalse = context.Build(condition.IfFalse);
            MugenBindingExtensions.Convert(ref ifTrue, ref ifFalse, true);
            return Expression.Condition(context.Build(condition.Condition).ConvertIfNeed(typeof(bool), true), ifTrue, ifFalse);
        }

        #endregion
    }
}