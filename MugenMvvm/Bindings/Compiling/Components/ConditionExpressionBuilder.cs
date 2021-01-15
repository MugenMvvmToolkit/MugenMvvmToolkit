using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class ConditionExpressionBuilder : IExpressionBuilderComponent, IHasPriority
    {
        public int Priority { get; set; } = CompilingComponentPriority.Condition;

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (expression is not IConditionExpressionNode condition)
                return null;

            var ifTrue = context.Build(condition.IfTrue);
            var ifFalse = context.Build(condition.IfFalse);
            BindingMugenExtensions.Convert(ref ifTrue, ref ifFalse, true);
            return Expression.Condition(context.Build(condition.Condition).ConvertIfNeed(typeof(bool), true), ifTrue, ifFalse);
        }
    }
}